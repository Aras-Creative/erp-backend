using ArasERP.Modules.Inventory.Application.Abstractions;
using ArasERP.Modules.Inventory.Domain.StockMovements;
using Microsoft.EntityFrameworkCore;

namespace ArasERP.Modules.Inventory.Infrastructure.Persistence.Repositories;

public sealed class StockMovementRepository(InventoryDbContext dbContext) : IStockMovementRepository
{
    public async Task AddAsync(
        StockMovement stockMovement,
        CancellationToken cancellationToken = default
    )
    {
        await dbContext.StockMovements.AddAsync(stockMovement, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}