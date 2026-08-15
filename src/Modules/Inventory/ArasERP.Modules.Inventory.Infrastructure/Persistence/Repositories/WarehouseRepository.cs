using ArasERP.Modules.Inventory.Application.Abstractions;
using ArasERP.Modules.Inventory.Domain.Warehouses;
using Microsoft.EntityFrameworkCore;

namespace ArasERP.Modules.Inventory.Infrastructure.Persistence.Repositories;

public sealed class WarehouseRepository : IWarehouseRepository
{
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
