using ArasERP.BuildingBlocks.Application;
using ArasERP.BuildingBlocks.Presentation;
using ArasERP.Modules.Inventory.Api.Response;
using ArasERP.Modules.Inventory.Application.StockMovements.List;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace ArasERP.Modules.Inventory.Api.Endpoints;

public static class StockMovementEndpoints
{
    public static IEndpointRouteBuilder MapStockMovementEndpoints(
        this IEndpointRouteBuilder endpoints
    )
    {
        var stockMovements = endpoints.MapGroup("api/stock-movements").WithTags("StockMovements");

        stockMovements.MapGet("/", ListStockMovements).WithName(nameof(ListStockMovements));

        return endpoints;
    }

    private static async Task<Ok<ApiResponse<PagedList<StockMovementResponse>>>> ListStockMovements(
        HttpContext context,
        IQueryHandler<ListStockMovementsQuery, PagedList<ListStockMovementsDto>> handler,
        CancellationToken cancellationToken,
        [FromQuery] Guid? itemId = null,
        [FromQuery] Guid? batchId = null,
        [FromQuery] Guid? warehouseId = null,
        [FromQuery] int page = PaginationDefaults.Page,
        [FromQuery] int pageSize = PaginationDefaults.PageSize,
        [FromQuery] string? orderBy = null,
        [FromQuery] bool descending = false
    )
    {
        page = Math.Max(page, PaginationDefaults.Page);
        pageSize = Math.Clamp(pageSize, 1, PaginationDefaults.MaxPageSize);

        var query = new ListStockMovementsQuery
        {
            ItemId = itemId,
            BatchId = batchId,
            WarehouseId = warehouseId,
            Page = page,
            PageSize = pageSize,
            OrderBy = orderBy,
            Descending = descending,
        };

        var paged = await handler.Handle(query, cancellationToken);

        return TypedResults.Ok(ApiResponse.Success(context, paged.ToResponse()));
    }
}
