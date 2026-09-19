using ArasERP.BuildingBlocks.Application;
using ArasERP.Modules.Inventory.Application.Abstractions;
using ArasERP.Modules.Inventory.Application.StockMovements.List;
using ArasERP.Modules.Inventory.Domain.Batches;
using ArasERP.Modules.Inventory.Domain.StockItems;
using ArasERP.Modules.Inventory.Domain.StockMovements;
using ArasERP.Modules.Inventory.Domain.Warehouses;
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

    public async Task<PagedList<ListStockMovementsDto>> ListAsync(
        StockMovementListFilter filter,
        CancellationToken cancellationToken = default
    )
    {
        var query =
            from m in dbContext.StockMovements.AsNoTracking()
            join item in dbContext.StockItems.AsNoTracking() on m.ItemId equals item.Id
            join warehouse in dbContext.Warehouses.AsNoTracking().IgnoreQueryFilters()
                on m.WarehouseId equals warehouse.Id
            select new
            {
                Movement = m,
                ItemName = item.Name,
                WarehouseName = warehouse.Name,
            };

        if (filter.ItemId.HasValue)
        {
            var itemId = new StockItemId(filter.ItemId.Value);
            query = query.Where(x => x.Movement.ItemId == itemId);
        }

        if (filter.BatchId.HasValue)
        {
            var batchId = new BatchId(filter.BatchId.Value);
            query = query.Where(x => x.Movement.BatchId != null && x.Movement.BatchId == batchId);
        }

        if (filter.WarehouseId.HasValue)
        {
            var warehouseId = new WarehouseId(filter.WarehouseId.Value);
            query = query.Where(x => x.Movement.WarehouseId == warehouseId);
        }

        var ordered = filter.OrderBy?.ToLowerInvariant() switch
        {
            "quantity" => filter.Descending
                ? query.OrderByDescending(x => x.Movement.Quantity)
                : query.OrderBy(x => x.Movement.Quantity),

            "direction" => filter.Descending
                ? query.OrderByDescending(x => x.Movement.Direction)
                : query.OrderBy(x => x.Movement.Direction),

            "sourcetype" => filter.Descending
                ? query.OrderByDescending(x => x.Movement.SourceType)
                : query.OrderBy(x => x.Movement.SourceType),

            _ => filter.Descending
                ? query.OrderByDescending(x => x.Movement.CreatedAt)
                : query.OrderBy(x => x.Movement.CreatedAt),
        };

        var totalCount = await ordered.CountAsync(cancellationToken);

        var rows = await ordered
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync(cancellationToken);

        var items = rows.Select(x => new ListStockMovementsDto
            {
                Id = x.Movement.Id.Value,
                ItemId = x.Movement.ItemId.Value,
                ItemName = x.ItemName,
                WarehouseId = x.Movement.WarehouseId.Value,
                WarehouseName = x.WarehouseName,
                BatchId = x.Movement.BatchId?.Value,
                Direction = x.Movement.Direction.Value,
                Quantity = x.Movement.Quantity,
                SourceType = x.Movement.SourceType.Value,
                SourceReferenceId = x.Movement.SourceReferenceId,
                ExternalReferenceNo = x.Movement.ExternalReferenceNo,
                Note = x.Movement.Note,
                CreatedAt = x.Movement.CreatedAt,
                RecordedBy = x.Movement.RecordedBy,
                ReceivedBy = x.Movement.ReceivedBy,
            })
            .ToList();

        return new PagedList<ListStockMovementsDto>(
            items,
            filter.Page,
            filter.PageSize,
            totalCount
        );
    }
}
