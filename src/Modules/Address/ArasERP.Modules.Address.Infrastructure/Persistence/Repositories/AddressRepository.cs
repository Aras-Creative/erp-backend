using System.Linq.Expressions;
using ArasERP.Modules.Address.Application.Abstractions;
using ArasERP.Modules.Address.Contracts.Addresses;
using ArasERP.Modules.Address.Domain;
using Microsoft.EntityFrameworkCore;
using NpgsqlTypes;

namespace ArasERP.Modules.Address.Infrastructure.Persistence.Repositories;

public sealed class AddressRepository : IAddressRepository
{
    private readonly AddressDbContext _dbContext;

    public AddressRepository(AddressDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Domain.Address?> GetByIdAsync(
        AddressId id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Addresses.FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Domain.Address>> GetByCodesAsync(
        IReadOnlyCollection<(string DestinationCode, string OriginCode)> codePairs,
        CancellationToken cancellationToken = default)
    {
        if (codePairs.Count == 0)
        {
            return [];
        }

        var pairs = codePairs.Select(p => (p.DestinationCode, p.OriginCode)).ToList();

        return await _dbContext
            .Addresses.Where(BuildCodePredicate(pairs))
            .ToListAsync(cancellationToken);
    }

    private static Expression<Func<Domain.Address, bool>> BuildCodePredicate(
        IReadOnlyCollection<(string DestinationCode, string OriginCode)> codePairs)
    {
        var parameter = Expression.Parameter(typeof(Domain.Address), "a");
        Expression? body = null;

        foreach (var pair in codePairs)
        {
            Expression condition = Expression.AndAlso(
                Expression.Equal(
                    Expression.Property(parameter, nameof(Domain.Address.DestinationCode)),
                    Expression.Constant(pair.DestinationCode)),
                Expression.Equal(
                    Expression.Property(parameter, nameof(Domain.Address.OriginCode)),
                    Expression.Constant(pair.OriginCode)));

            body = body is null ? condition : Expression.OrElse(body, condition);
        }

        return body is null
            ? _ => false
            : Expression.Lambda<Func<Domain.Address, bool>>(body, parameter);
    }

    public async Task<IReadOnlyList<AddressSearchResultItemDto>> SearchAsync(
        string keyword,
        int limit,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(keyword))
        {
            return [];
        }

        return await _dbContext
            .Addresses.Where(
                a =>
                    EF.Property<NpgsqlTsVector>(a, "SearchVector")
                        .Matches(EF.Functions.PlainToTsQuery("simple", keyword)))
            .OrderBy(a => a.ProvinceName)
            .ThenBy(a => a.CityName)
            .ThenBy(a => a.DistrictName)
            .ThenBy(a => a.SubDistrictName)
            .Take(limit)
            .Select(
                a => new AddressSearchResultItemDto
                {
                    AddressId = a.Id.Value,
                    ExternalId = a.ExternalId,
                    DestinationCode = a.DestinationCode,
                    OriginCode = a.OriginCode,
                    ProvinceName = a.ProvinceName,
                    CityName = a.CityName,
                    DistrictName = a.DistrictName,
                    SubDistrictName = a.SubDistrictName,
                    ZipCode = a.ZipCode,
                })
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Domain.Address address, CancellationToken cancellationToken = default)
    {
        await _dbContext.Addresses.AddAsync(address, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpsertRangeAsync(
        IReadOnlyCollection<Domain.Address> addresses,
        CancellationToken cancellationToken = default)
    {
        if (addresses.Count == 0)
        {
            return;
        }

        var sql = """
            INSERT INTO addresses (id, destination_code, origin_code, province_name, city_name, district_name, sub_district_name, zip_code, external_id, created_at_utc, updated_at_utc)
            VALUES (@p0, @p1, @p2, @p3, @p4, @p5, @p6, @p7, @p8, @p9, @p9)
            ON CONFLICT (external_id) WHERE external_id IS NOT NULL DO UPDATE SET
                destination_code = EXCLUDED.destination_code,
                origin_code = EXCLUDED.origin_code,
                province_name = EXCLUDED.province_name,
                city_name = EXCLUDED.city_name,
                district_name = EXCLUDED.district_name,
                sub_district_name = EXCLUDED.sub_district_name,
                zip_code = EXCLUDED.zip_code,
                updated_at_utc = EXCLUDED.updated_at_utc
            """;

        var now = DateTime.UtcNow;

        foreach (var address in addresses)
        {
            if (string.IsNullOrWhiteSpace(address.ExternalId))
            {
                continue;
            }

            await _dbContext.Database.ExecuteSqlRawAsync(
                sql,
                [
                    address.Id.Value,
                    address.DestinationCode,
                    address.OriginCode,
                    address.ProvinceName,
                    address.CityName,
                    address.DistrictName,
                    address.SubDistrictName,
                    address.ZipCode,
                    address.ExternalId,
                    now,
                ],
                cancellationToken);
        }
    }

    public async Task AddRangeAsync(
        IReadOnlyCollection<Domain.Address> addresses,
        CancellationToken cancellationToken = default)
    {
        if (addresses.Count == 0)
        {
            return;
        }

        await _dbContext.Addresses.AddRangeAsync(addresses, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(
        Domain.Address address,
        CancellationToken cancellationToken = default)
    {
        _dbContext.Addresses.Update(address);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateRangeAsync(
        IReadOnlyCollection<Domain.Address> addresses,
        CancellationToken cancellationToken = default)
    {
        if (addresses.Count == 0)
        {
            return;
        }

        _dbContext.Addresses.UpdateRange(addresses);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
