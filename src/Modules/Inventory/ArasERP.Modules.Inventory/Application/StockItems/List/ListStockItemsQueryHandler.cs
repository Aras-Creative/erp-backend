using ArasERP.BuildingBlocks.Application;
using ArasERP.Modules.Inventory.Application.Abstractions;
using FluentValidation;

namespace ArasERP.Modules.Inventory.Application.StockItems.List;

public sealed class ListStockItemsQueryHandler(
    IStockItemRepository stockItemRepository,
    IValidator<ListStockItemsQuery> validator
) : IQueryHandler<ListStockItemsQuery, PagedList<ListStockItemsDto>>
{
    public async Task<PagedList<ListStockItemsDto>> Handle(
        ListStockItemsQuery query,
        CancellationToken cancellationToken = default
    )
    {
        await validator.ValidateAndThrowAsync(query, cancellationToken);

        var filter = new StockItemListFilter(
            query.Search,
            query.IsActive,
            query.Page,
            query.PageSize,
            query.OrderBy,
            query.Descending,
            query.WarehouseId
        );

        return await stockItemRepository.ListAsync(filter, cancellationToken);
    }
}
