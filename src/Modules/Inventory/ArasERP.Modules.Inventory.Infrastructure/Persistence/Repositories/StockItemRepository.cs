using ArasERP.BuildingBlocks.Application;
using ArasERP.Modules.Inventory.Application.Abstractions;
using ArasERP.Modules.Inventory.Application.StockItems.List;
using ArasERP.Modules.Inventory.Domain.StockItems;
using ArasERP.Modules.Inventory.Domain.StockLevels;
using ArasERP.Modules.Inventory.Domain.Warehouses;
using Microsoft.EntityFrameworkCore;

namespace ArasERP.Modules.Inventory.Infrastructure.Persistence.Repositories;

public sealed class StockItemRepository(InventoryDbContext dbContext) : IStockItemRepository
{
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
        IQueryable<StockItem> itemQuery = dbContext.StockItems.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var search = filter.Search.Trim();
            itemQuery = itemQuery.Where(x => x.Sku.Contains(search) || x.Name.Contains(search));
        }

        if (filter.IsActive.HasValue)
        {
            itemQuery = itemQuery.Where(x => x.IsActive == filter.IsActive.Value);
        }

        IQueryable<StockLevel> levelQuery = dbContext.StockLevels.AsNoTracking();
        if (filter.WarehouseId.HasValue)
        {
            var warehouseId = new WarehouseId(filter.WarehouseId.Value);
            levelQuery = levelQuery.Where(l => l.WarehouseId == warehouseId);
        }

        var joined =
            from item in itemQuery
            join level in levelQuery on item.Id equals level.ItemId into levelGroup
            from level in levelGroup.DefaultIfEmpty()
            select new { Item = item, Level = level };

        var projected =
            from row in joined
            group row by new
            {
                Id = row.Item.Id.Value,
                row.Item.Sku,
                row.Item.Name,
                row.Item.Unit,
                row.Item.IsActive,
                row.Item.CostingMethod,
            } into g
            select new
            {
                g.Key.Id,
                g.Key.Sku,
                g.Key.Name,
                g.Key.Unit,
                g.Key.IsActive,
                CostingMethod = g.Key.CostingMethod.Value,
                OnHandQty = g.Sum(r => r.Level != null ? r.Level.OnHandQty : 0m),
                ReservedQty = g.Sum(r => r.Level != null ? r.Level.ReservedQty : 0m),
            };

        projected = filter.OrderBy?.ToLowerInvariant() switch
        {
            "sku" => filter.Descending
                ? projected.OrderByDescending(x => x.Sku)
                : projected.OrderBy(x => x.Sku),

            "costingmethod" => filter.Descending
                ? projected.OrderByDescending(x => x.CostingMethod)
                : projected.OrderBy(x => x.CostingMethod),

            _ => filter.Descending
                ? projected.OrderByDescending(x => x.Name)
                : projected.OrderBy(x => x.Name),
        };

        var totalCount = await projected.CountAsync(cancellationToken);

        var items = await projected
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(x => new ListStockItemsDto
            {
                Id = x.Id,
                Sku = x.Sku,
                Name = x.Name,
                Unit = x.Unit,
                IsActive = x.IsActive,
                OnHandQty = x.OnHandQty,
                ReservedQty = x.ReservedQty,
                AvailableQty = x.OnHandQty - x.ReservedQty,
            })
            .ToListAsync(cancellationToken);

        return new PagedList<ListStockItemsDto>(items, filter.Page, filter.PageSize, totalCount);
    }
}
