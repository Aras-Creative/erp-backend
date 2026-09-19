using System.Linq.Expressions;
using ArasERP.BuildingBlocks.Application;
using ArasERP.Modules.Inventory.Application.Abstractions;
using ArasERP.Modules.Inventory.Application.Warehouses.GetById;
using ArasERP.Modules.Inventory.Application.Warehouses.GetOptions;
using ArasERP.Modules.Inventory.Application.Warehouses.List;
using ArasERP.Modules.Inventory.Domain.Warehouses;
using Microsoft.EntityFrameworkCore;

namespace ArasERP.Modules.Inventory.Infrastructure.Persistence.Repositories;

public sealed class WarehouseRepository(InventoryDbContext dbContext) : IWarehouseRepository
{
    private static readonly Expression<
        Func<Warehouse, WarehouseDetailDto>
    > WarehouseDetailProjection = w => new WarehouseDetailDto
    {
        WarehouseId = w.Id.Value,
        Name = w.Name,
        IsActive = w.IsActive,
        PersonInCharge = new WarehouseDetailDto.PersonInChargeData
        {
            Name = w.PersonInCharge.Name,
            Phone = w.PersonInCharge.Phone,
        },
        Address = new WarehouseDetailDto.AddressData
        {
            AddressId = w.Address.AddressId,
            SubDistrictName = w.Address.SubDistrictName,
            DistrictName = w.Address.DistrictName,
            CityName = w.Address.CityName,
            ProvinceName = w.Address.ProvinceName,
            ZipCode = w.Address.ZipCode,
        },
        FullAddressText = w.FullAddressText,
    };

    public async Task<bool> ExistsByNameAsync(
        string name,
        WarehouseId? excludeId = null,
        CancellationToken cancellationToken = default
    )
    {
        return await dbContext.Warehouses.AnyAsync(
            w => w.Name == name && (excludeId == null || w.Id != excludeId),
            cancellationToken
        );
    }

    public async Task<Warehouse?> GetByIdAsync(
        WarehouseId id,
        CancellationToken cancellationToken = default
    )
    {
        return await dbContext.Warehouses.FirstOrDefaultAsync(w => w.Id == id, cancellationToken);
    }

    public async Task<bool> IsActiveAsync(
        WarehouseId id,
        CancellationToken cancellationToken = default
    )
    {
        return await dbContext.Warehouses.AnyAsync(
            w => w.Id == id && w.IsActive,
            cancellationToken
        );
    }

    public async Task<WarehouseDetailDto?> GetDetailAsync(
        Guid warehouseId,
        CancellationToken cancellationToken = default
    )
    {
        return await dbContext
            .Warehouses.Where(w => w.Id == new WarehouseId(warehouseId))
            .Select(WarehouseDetailProjection)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<PagedList<WarehouseListItemDto>> GetPagedAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default
    )
    {
        var query = dbContext.Warehouses.OrderBy(w => w.Name);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(WarehouseDetailProjection)
            .ToListAsync(cancellationToken);

        return new PagedList<WarehouseListItemDto>(
            items.Select(WarehouseListItemDto.FromDetail).ToList(),
            page,
            pageSize,
            totalCount
        );
    }

    public async Task<IReadOnlyList<WarehouseOptionDto>> GetOptionsAsync(
        CancellationToken cancellationToken = default
    )
    {
        return await dbContext
            .Warehouses.Where(w => !w.IsDeleted && w.IsActive)
            .OrderBy(w => w.Name)
            .Select(w => new WarehouseOptionDto(w.Id.Value, w.Name))
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Warehouse warehouse, CancellationToken cancellationToken = default)
    {
        await dbContext.Warehouses.AddAsync(warehouse, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(
        Warehouse warehouse,
        CancellationToken cancellationToken = default
    )
    {
        dbContext.Warehouses.Update(warehouse);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
