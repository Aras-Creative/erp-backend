using ArasERP.BuildingBlocks.Application;
using ArasERP.Modules.Inventory.Domain.StockItems;

namespace ArasERP.Modules.Inventory.Application.Abstractions;

public sealed record StockItemListFilter(
    string? Search,
    bool? IsActive,
    int Page,
    int PageSize,
    string? OrderBy,
    bool Descending
);

public interface IStockItemRepository
{
    Task AddAsync(StockItem stockItem, CancellationToken cancellationToken = default);
    Task<bool> ExistsBySkuAsync(
        string sku,
        StockItemId? excludeId,
        CancellationToken cancellationToken = default
    );
    Task<bool> IsActiveAsync(StockItemId id, CancellationToken cancellationToken = default);

    Task<PagedList<StockItem>> ListAsync(
        StockItemListFilter filter,
        CancellationToken cancellationToken = default
    );
}
