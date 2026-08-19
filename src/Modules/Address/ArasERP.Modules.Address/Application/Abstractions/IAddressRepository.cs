namespace ArasERP.Modules.Address.Application.Abstractions;

public interface IAddressRepository
{
    Task<Domain.Address?> GetByIdAsync(
        Domain.AddressId id,
        CancellationToken cancellationToken = default
    );
    Task<IReadOnlyList<Domain.Address>> GetByCodesAsync(
        IReadOnlyCollection<(string DestinationCode, string OriginCode)> codePairs,
        CancellationToken cancellationToken = default
    );
    Task<IReadOnlyList<Domain.Address>> SearchAsync(
        string keyword,
        int limit,
        CancellationToken cancellationToken = default
    );
    Task UpsertRangeAsync(
        IReadOnlyCollection<Domain.Address> addresses,
        CancellationToken cancellationToken = default
    );
    Task AddAsync(Domain.Address address, CancellationToken cancellationToken = default);
    Task AddRangeAsync(
        IReadOnlyCollection<Domain.Address> addresses,
        CancellationToken cancellationToken = default
    );
    Task UpdateAsync(Domain.Address address, CancellationToken cancellationToken = default);
    Task UpdateRangeAsync(
        IReadOnlyCollection<Domain.Address> addresses,
        CancellationToken cancellationToken = default
    );
}
