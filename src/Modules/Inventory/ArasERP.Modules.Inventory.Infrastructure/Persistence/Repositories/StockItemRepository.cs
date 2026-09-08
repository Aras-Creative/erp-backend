using ArasERP.BuildingBlocks.Application;
using ArasERP.Modules.Inventory.Application.Abstractions;
using ArasERP.Modules.Inventory.Domain.StockItems;
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

    public async Task<PagedList<StockItem>> ListAsync(
        StockItemListFilter filter,
        CancellationToken cancellationToken = default
    )
    {
        IQueryable<StockItem> query = dbContext.StockItems.AsNoTracking();

        // Search
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var search = filter.Search.Trim();

            query = query.Where(x => x.Sku.Contains(search) || x.Name.Contains(search));
        }

        // Active filter
        if (filter.IsActive.HasValue)
        {
            query = query.Where(x => x.IsActive == filter.IsActive.Value);
        }

        // Sorting
        query = filter.OrderBy?.ToLowerInvariant() switch
        {
            "sku" => filter.Descending
                ? query.OrderByDescending(x => x.Sku)
                : query.OrderBy(x => x.Sku),

            "name" => filter.Descending
                ? query.OrderByDescending(x => x.Name)
                : query.OrderBy(x => x.Name),

            "costingmethod" => filter.Descending
                ? query.OrderByDescending(x => x.CostingMethod.Value)
                : query.OrderBy(x => x.CostingMethod.Value),

            _ => query.OrderBy(x => x.Name),
        };

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedList<StockItem>(items, totalCount, filter.Page, filter.PageSize);
    }
}
