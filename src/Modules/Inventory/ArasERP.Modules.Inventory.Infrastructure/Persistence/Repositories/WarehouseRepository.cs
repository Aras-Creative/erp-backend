using System.Linq.Expressions;
using ArasERP.Modules.Inventory.Application.Abstractions;
using ArasERP.Modules.Inventory.Application.Warehouses.GetById;
using ArasERP.Modules.Inventory.Application.Warehouses.GetOptions;
using ArasERP.Modules.Inventory.Application.Warehouses.List;
using ArasERP.Modules.Inventory.Domain.Warehouses;
using Microsoft.EntityFrameworkCore;

namespace ArasERP.Modules.Inventory.Infrastructure.Persistence.Repositories;

public sealed class WarehouseRepository : IWarehouseRepository
{
    private static readonly Expression<
        Func<Warehouse, WarehouseDetailDto>
    > WarehouseDetailProjection = w => new WarehouseDetailDto
    {
        WarehouseId = w.Id.Value,
        Name = w.Name,
        PersonInCharge = new WarehouseDetailDto.PersonInChargeData
        {
            Name = w.PersonInCharge.Name,
            Phone = w.PersonInCharge.Phone,
        },
        Address = new WarehouseDetailDto.AddressData
        {
            Street = w.Address.Street,
            City = w.Address.City,
            State = w.Address.State,
            PostalCode = w.Address.PostalCode,
            Country = w.Address.Country,
        },
        FullAddressText = w.FullAddressText,
    };

    private readonly InventoryDbContext _dbContext;

    public WarehouseRepository(InventoryDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> ExistsByNameAsync(
        string name,
        WarehouseId? excludeId = null,
        CancellationToken cancellationToken = default
    )
    {
        return await _dbContext.Warehouses.AnyAsync(
            w => w.Name == name && (excludeId == null || w.Id != excludeId),
            cancellationToken
        );
    }

    public async Task<Warehouse?> GetByIdAsync(
        WarehouseId id,
        CancellationToken cancellationToken = default
    )
    {
        return await _dbContext.Warehouses.FirstOrDefaultAsync(w => w.Id == id, cancellationToken);
    }

    public async Task<WarehouseDetailDto?> GetDetailAsync(
        Guid warehouseId,
        CancellationToken cancellationToken = default
    )
    {
        return await _dbContext
            .Warehouses.Where(w => w.Id == new WarehouseId(warehouseId))
            .Select(WarehouseDetailProjection)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<WarehouseListItemDto>> GetAllAsync(
        CancellationToken cancellationToken = default
    )
    {
        return (
            await _dbContext
                .Warehouses.OrderBy(w => w.Name)
                .Select(WarehouseDetailProjection)
                .ToListAsync(cancellationToken)
        )
            .Select(WarehouseListItemDto.FromDetail)
            .ToList();
    }

    public async Task<IReadOnlyList<WarehouseOptionDto>> GetOptionsAsync(
        CancellationToken cancellationToken = default
    )
    {
        return await _dbContext
            .Warehouses.Where(w => !w.IsDeleted)
            .OrderBy(w => w.Name)
            .Select(w => new WarehouseOptionDto(w.Id.Value, w.Name))
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Warehouse warehouse, CancellationToken cancellationToken = default)
    {
        await _dbContext.Warehouses.AddAsync(warehouse, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(
        Warehouse warehouse,
        CancellationToken cancellationToken = default
    )
    {
        _dbContext.Warehouses.Update(warehouse);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
