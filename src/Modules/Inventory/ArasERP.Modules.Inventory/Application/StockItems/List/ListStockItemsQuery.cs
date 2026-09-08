using ArasERP.BuildingBlocks.Application;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ArasERP.Modules.Inventory.Application.StockItems.List;

public sealed class ListStockItemsQuery : IQuery, IQuery<PagedList<ListStockItemsDto>>
{
    public string? Search { get; set; }

    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;

    public string? OrderBy { get; set; }
    public bool Descending { get; set; }

    public bool? IsActive { get; set; }
}
