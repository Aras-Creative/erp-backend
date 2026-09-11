using ArasERP.Modules.Inventory.Domain.StockMovements;

namespace ArasERP.Modules.Inventory.Application.Abstractions;

public interface IStockMovementRepository
{
    Task AddAsync(StockMovement stockMovement, CancellationToken cancellationToken = default);
}