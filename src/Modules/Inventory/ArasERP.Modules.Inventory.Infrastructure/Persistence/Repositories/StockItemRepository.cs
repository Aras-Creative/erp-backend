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
            join warehouse in dbContext.Warehouses.AsNoTracking()
                on level.WarehouseId equals warehouse.Id
                into warehouseGroup
            from warehouse in warehouseGroup.DefaultIfEmpty()
            select new
            {
                Item = item,
                Level = level,
                Warehouse = warehouse,
            };

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
                WarehouseId = row.Level != null ? row.Level.WarehouseId.Value : (Guid?)null,
                WarehouseName = row.Warehouse != null ? row.Warehouse.Name : null,
                OnHandQty = row.Level != null ? row.Level.OnHandQty : 0m,
                ReservedQty = row.Level != null ? row.Level.ReservedQty : 0m,
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
                WarehouseId = x.WarehouseId ?? filter.WarehouseId,
                WarehouseName = x.WarehouseName ?? filteredWarehouseName,
                OnHandQty = x.OnHandQty,
                ReservedQty = x.ReservedQty,
                AvailableQty = x.OnHandQty - x.ReservedQty,
            })
            .ToList();

        return new PagedList<ListStockItemsDto>(items, filter.Page, filter.PageSize, totalCount);
    }
}
