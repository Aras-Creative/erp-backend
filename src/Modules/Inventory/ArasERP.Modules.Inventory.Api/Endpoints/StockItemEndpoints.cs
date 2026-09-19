using ArasERP.BuildingBlocks.Application;
using ArasERP.BuildingBlocks.Presentation;
using ArasERP.Modules.Inventory.Api.Response;
using ArasERP.Modules.Inventory.Application.StockItems.Create;
using ArasERP.Modules.Inventory.Application.StockItems.List;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace ArasERP.Modules.Inventory.Api.Endpoints;

public static class StockItemEndpoints
{
    public static IEndpointRouteBuilder MapStockItemEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var stockItems = endpoints.MapGroup("api/stock-items").WithTags("StockItems");

        stockItems.MapPost("/", CreateStockItems).WithName(nameof(CreateStockItems));

        stockItems.MapGet("/", ListStockItems).WithName(nameof(ListStockItems));

        return endpoints;
    }

    private static async Task<Created> CreateStockItems(
        CreateStockItemCommand command,
        IMediator mediator,
        CancellationToken cancellationToken
    )
    {
        await mediator.SendAsync<CreateStockItemCommand>(command, cancellationToken);
        return TypedResults.Created();
    }

    private static async Task<Ok<ApiResponse<PagedList<StockItemResponse>>>> ListStockItems(
        HttpContext context,
        IQueryHandler<ListStockItemsQuery, PagedList<ListStockItemsDto>> handler,
        CancellationToken cancellationToken,
        [FromQuery] string? search = null,
        [FromQuery] bool? isActive = null,
        [FromQuery] int page = PaginationDefaults.Page,
        [FromQuery] int pageSize = PaginationDefaults.PageSize,
        [FromQuery] string? orderBy = null,
        [FromQuery] bool descending = false,
        [FromQuery] Guid? warehouseId = null
    )
    {
        page = Math.Max(page, PaginationDefaults.Page);
        pageSize = Math.Clamp(pageSize, 1, PaginationDefaults.MaxPageSize);

        var query = new ListStockItemsQuery
        {
            Search = search,
            IsActive = isActive,
            Page = page,
            PageSize = pageSize,
            OrderBy = orderBy,
            Descending = descending,
            WarehouseId = warehouseId,
        };

        var paged = await handler.Handle(query, cancellationToken);

        return TypedResults.Ok(ApiResponse.Success(context, paged.ToResponse()));
    }
}
