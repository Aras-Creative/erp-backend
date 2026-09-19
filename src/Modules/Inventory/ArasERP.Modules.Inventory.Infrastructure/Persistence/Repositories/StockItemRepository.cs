using ArasERP.BuildingBlocks.Application;
using ArasERP.Modules.Inventory.Application.Abstractions;
using ArasERP.Modules.Inventory.Application.StockItems.List;
using ArasERP.Modules.Inventory.Domain.StockItems;
using ArasERP.Modules.Inventory.Domain.Warehouses;
using Microsoft.EntityFrameworkCore;

namespace ArasERP.Modules.Inventory.Infrastructure.Persistence.Repositories;

public sealed class StockItemRepository(InventoryDbContext dbContext) : IStockItemRepository
{
    private sealed record AggregatedStockLevel
    {
        public StockItemId ItemId { get; init; } = null!;

        public decimal OnHandQty { get; init; }

        public decimal ReservedQty { get; init; }
    }

    public async Task AddAsync(StockItem stockItem, CancellationToken cancellationToken = default)
    {
        await dbContext.StockItems.AddAsync(stockItem, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> ExistsBySkuAsync(
        string sku,
        StockItemId? excludeId,
        CancellationToken cancellationToken = default
    )
    {
        return await dbContext.StockItems.AnyAsync(
            w => w.Sku == sku && (excludeId == null || w.Id != excludeId),
            cancellationToken
        );
    }

    public async Task<bool> IsActiveAsync(
        StockItemId id,
        CancellationToken cancellationToken = default
    )
    {
        return await dbContext.StockItems.AnyAsync(
            w => w.Id == id && w.IsActive,
            cancellationToken
        );
    }

    public async Task<PagedList<ListStockItemsDto>> ListAsync(
        StockItemListFilter filter,
        CancellationToken cancellationToken = default
    )
    {
        string? filteredWarehouseName = null;
        if (filter.WarehouseId.HasValue)
        {
            filteredWarehouseName = await dbContext
                .Warehouses.AsNoTracking()
                .Where(w => w.Id == new WarehouseId(filter.WarehouseId.Value))
                .Select(w => w.Name)
                .FirstOrDefaultAsync(cancellationToken);
        }

        IQueryable<StockItem> itemQuery = dbContext.StockItems.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var search = filter.Search.Trim();
            itemQuery = itemQuery.Where(x =>
                EF.Functions.ILike(x.Sku, $"%{search}%")
                || EF.Functions.ILike(x.Name, $"%{search}%")
            );
        }

        if (filter.IsActive.HasValue)
        {
            itemQuery = itemQuery.Where(x => x.IsActive == filter.IsActive.Value);
        }

        IQueryable<AggregatedStockLevel> levelQuery;

        if (filter.WarehouseId.HasValue)
        {
            var warehouseId = new WarehouseId(filter.WarehouseId.Value);
            levelQuery = dbContext
                .StockLevels.AsNoTracking()
                .Where(l => l.WarehouseId == warehouseId)
                .Select(l => new AggregatedStockLevel
                {
                    ItemId = l.ItemId,
                    OnHandQty = l.OnHandQty,
                    ReservedQty = l.ReservedQty,
                });
        }
        else
        {
            levelQuery = dbContext
                .StockLevels.AsNoTracking()
                .GroupBy(l => l.ItemId)
                .Select(g => new AggregatedStockLevel
                {
                    ItemId = g.Key,
                    OnHandQty = g.Sum(l => l.OnHandQty),
                    ReservedQty = g.Sum(l => l.ReservedQty),
                });
        }

        var joined =
            from item in itemQuery
            join level in levelQuery on item.Id equals level.ItemId into levelGroup
            from level in levelGroup.DefaultIfEmpty()
            select new { Item = item, Level = level };

        var projected =
            from row in joined
            select new
            {
                Id = row.Item.Id.Value,
                row.Item.Sku,
                row.Item.Name,
                row.Item.Unit,
                row.Item.IsActive,
                CostingMethod = row.Item.CostingMethod.Value,
                row.Item.CreatedAt,
                row.Item.UpdatedAt,
                OnHandQty = row.Level != null ? row.Level.OnHandQty : 0m,
                ReservedQty = row.Level != null ? row.Level.ReservedQty : 0m,
            };

        projected = filter.OrderBy?.ToLowerInvariant() switch
        {
            "sku" => filter.Descending
                ? projected.OrderByDescending(x => x.Sku)
                : projected.OrderBy(x => x.Sku),

            "name" => filter.Descending
                ? projected.OrderByDescending(x => x.Name)
                : projected.OrderBy(x => x.Name),

            "isactive" => filter.Descending
                ? projected.OrderByDescending(x => x.IsActive)
                : projected.OrderBy(x => x.IsActive),

            "costingmethod" => filter.Descending
                ? projected.OrderByDescending(x => x.CostingMethod)
                : projected.OrderBy(x => x.CostingMethod),

            "onhandqty" => filter.Descending
                ? projected.OrderByDescending(x => x.OnHandQty)
                : projected.OrderBy(x => x.OnHandQty),

            "createdat" => filter.Descending
                ? projected.OrderByDescending(x => x.CreatedAt)
                : projected.OrderBy(x => x.CreatedAt),

            "updatedat" => filter.Descending
                ? projected.OrderByDescending(x => x.UpdatedAt)
                : projected.OrderBy(x => x.UpdatedAt),

            _ => filter.Descending
                ? projected.OrderByDescending(x => x.Name)
                : projected.OrderBy(x => x.Name),
        };

        var totalCount = await projected.CountAsync(cancellationToken);

        var pageRows = await projected
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync(cancellationToken);

        var items = pageRows
            .Select(x => new ListStockItemsDto
            {
                Id = x.Id,
                Sku = x.Sku,
                Name = x.Name,
                Unit = x.Unit,
                IsActive = x.IsActive,
                WarehouseId = filter.WarehouseId,
                WarehouseName = filteredWarehouseName,
                OnHandQty = x.OnHandQty,
                ReservedQty = x.ReservedQty,
                AvailableQty = x.OnHandQty - x.ReservedQty,
            })
            .ToList();

        return new PagedList<ListStockItemsDto>(items, filter.Page, filter.PageSize, totalCount);
    }
}
