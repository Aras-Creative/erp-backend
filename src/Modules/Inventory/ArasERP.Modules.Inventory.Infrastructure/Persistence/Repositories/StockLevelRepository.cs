using ArasERP.BuildingBlocks.Application;
using ArasERP.Modules.Inventory.Application.Abstractions;
using ArasERP.Modules.Inventory.Domain.StockItems;
using ArasERP.Modules.Inventory.Domain.StockLevels;
using ArasERP.Modules.Inventory.Domain.Warehouses;
using Microsoft.EntityFrameworkCore;

namespace ArasERP.Modules.Inventory.Infrastructure.Persistence.Repositories;

public sealed class StockLevelRepository(InventoryDbContext dbContext) : IStockLevelRepository
{
    public async Task<StockLevel?> GetByKeyAsync(
        StockItemId itemId,
        WarehouseId warehouseId,
        CancellationToken cancellationToken = default
    )
    {
        return await dbContext.StockLevels.FirstOrDefaultAsync(
            l => l.ItemId == itemId && l.WarehouseId == warehouseId,
            cancellationToken
        );
    }

    public async Task AddAsync(StockLevel stockLevel, CancellationToken cancellationToken = default)
    {
        await dbContext.StockLevels.AddAsync(stockLevel, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(
        StockLevel stockLevel,
        CancellationToken cancellationToken = default
    )
    {
        dbContext.StockLevels.Update(stockLevel);
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ValidationException("Stock level was modified concurrently. Please retry.");
        }
    }

    public async Task<bool> HasStockAsync(
        WarehouseId warehouseId,
        CancellationToken cancellationToken = default
    )
    {
        return await dbContext.StockLevels.AnyAsync(
            l => l.WarehouseId == warehouseId && (l.OnHandQty > 0 || l.ReservedQty > 0),
            cancellationToken
        );
    }
}
