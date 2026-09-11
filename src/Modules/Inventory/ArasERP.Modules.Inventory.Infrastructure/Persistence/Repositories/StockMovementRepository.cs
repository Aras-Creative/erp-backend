using ArasERP.BuildingBlocks.Application;
using ArasERP.Modules.Inventory.Application.Abstractions;
using ArasERP.Modules.Inventory.Application.StockMovements.List;
using ArasERP.Modules.Inventory.Domain.Batches;
using ArasERP.Modules.Inventory.Domain.StockItems;
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

    public async Task<PagedList<ListStockMovementsDto>> ListAsync(
        StockMovementListFilter filter,
        CancellationToken cancellationToken = default
    )
    {
        IQueryable<StockMovement> query = dbContext.StockMovements.AsNoTracking();

        if (filter.ItemId.HasValue)
        {
            var itemId = new StockItemId(filter.ItemId.Value);
            query = query.Where(m => m.ItemId == itemId);
        }

        if (filter.BatchId.HasValue)
        {
            var batchId = new BatchId(filter.BatchId.Value);
            query = query.Where(m => m.BatchId != null && m.BatchId == batchId);
        }

        var ordered = filter.OrderBy?.ToLowerInvariant() switch
        {
            "quantity" => filter.Descending
                ? query.OrderByDescending(m => m.Quantity)
                : query.OrderBy(m => m.Quantity),

            "direction" => filter.Descending
                ? query.OrderByDescending(m => m.Direction)
                : query.OrderBy(m => m.Direction),

            "sourcetype" => filter.Descending
                ? query.OrderByDescending(m => m.SourceType)
                : query.OrderBy(m => m.SourceType),

            _ => filter.Descending
                ? query.OrderByDescending(m => m.CreatedAt)
                : query.OrderBy(m => m.CreatedAt),
        };

        var totalCount = await ordered.CountAsync(cancellationToken);

        var rows = await ordered
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync(cancellationToken);

        var items = rows
            .Select(m => new ListStockMovementsDto
            {
                Id = m.Id.Value,
                ItemId = m.ItemId.Value,
                WarehouseId = m.WarehouseId.Value,
                BatchId = m.BatchId?.Value,
                Direction = m.Direction.Value,
                Quantity = m.Quantity,
                SourceType = m.SourceType.Value,
                SourceReferenceId = m.SourceReferenceId,
                ExternalReferenceNo = m.ExternalReferenceNo,
                Note = m.Note,
                CreatedAt = m.CreatedAt,
                RecordedBy = m.RecordedBy,
                ReceivedBy = m.ReceivedBy,
            })
            .ToList();

        return new PagedList<ListStockMovementsDto>(items, filter.Page, filter.PageSize, totalCount);
    }
}