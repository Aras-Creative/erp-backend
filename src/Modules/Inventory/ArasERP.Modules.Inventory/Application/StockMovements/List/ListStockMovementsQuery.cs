using ArasERP.BuildingBlocks.Application;

namespace ArasERP.Modules.Inventory.Application.StockMovements.List;

public sealed class ListStockMovementsQuery : IQuery, IQuery<PagedList<ListStockMovementsDto>>
{
    public Guid? ItemId { get; set; }
    public Guid? BatchId { get; set; }

    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;

    public string? OrderBy { get; set; }
    public bool Descending { get; set; }
}