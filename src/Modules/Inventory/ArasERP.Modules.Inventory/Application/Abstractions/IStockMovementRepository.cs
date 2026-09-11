using ArasERP.BuildingBlocks.Application;
using ArasERP.Modules.Inventory.Application.StockMovements.List;
using ArasERP.Modules.Inventory.Domain.StockMovements;

namespace ArasERP.Modules.Inventory.Application.Abstractions;

public sealed record StockMovementListFilter(
    Guid? ItemId,
    Guid? BatchId,
    int Page,
    int PageSize,
    string? OrderBy,
    bool Descending
);

public interface IStockMovementRepository
{
    Task AddAsync(StockMovement stockMovement, CancellationToken cancellationToken = default);

    Task<PagedList<ListStockMovementsDto>> ListAsync(
        StockMovementListFilter filter,
        CancellationToken cancellationToken = default
    );
}