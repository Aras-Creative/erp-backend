using ArasERP.BuildingBlocks.Application;
using ArasERP.Modules.Inventory.Application.Abstractions;
using FluentValidation;

namespace ArasERP.Modules.Inventory.Application.StockMovements.List;

public sealed class ListStockMovementsQueryHandler(
    IStockMovementRepository stockMovementRepository,
    IValidator<ListStockMovementsQuery> validator
) : IQueryHandler<ListStockMovementsQuery, PagedList<ListStockMovementsDto>>
{
    public async Task<PagedList<ListStockMovementsDto>> Handle(
        ListStockMovementsQuery query,
        CancellationToken cancellationToken = default
    )
    {
        await validator.ValidateAndThrowAsync(query, cancellationToken);

        var filter = new StockMovementListFilter(
            query.ItemId,
            query.BatchId,
            query.Page,
            query.PageSize,
            query.OrderBy,
            query.Descending
        );

        return await stockMovementRepository.ListAsync(filter, cancellationToken);
    }
}