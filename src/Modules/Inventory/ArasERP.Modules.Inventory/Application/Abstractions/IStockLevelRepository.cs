using ArasERP.Modules.Inventory.Domain.StockItems;
using ArasERP.Modules.Inventory.Domain.StockLevels;
using ArasERP.Modules.Inventory.Domain.Warehouses;

namespace ArasERP.Modules.Inventory.Application.Abstractions;

public interface IStockLevelRepository
{
    Task<StockLevel?> GetByKeyAsync(
        StockItemId itemId,
        WarehouseId warehouseId,
        CancellationToken cancellationToken = default
    );
    Task AddAsync(StockLevel stockLevel, CancellationToken cancellationToken = default);
    Task UpdateAsync(StockLevel stockLevel, CancellationToken cancellationToken = default);
    Task<bool> HasStockAsync(
        WarehouseId warehouseId,
        CancellationToken cancellationToken = default
    );
}
