using ArasERP.BuildingBlocks.Application;
using ArasERP.Modules.Inventory.Application.StockItems.Create;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;

namespace ArasERP.Modules.Inventory.Api.StockItems.Endpoints;

public static class StockItemEndpoints
{
    public static IEndpointRouteBuilder MapStockItemEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var stockItems = endpoints.MapGroup("api/stock-items").WithTags("StockItems");

        stockItems.MapPost("/", CreateStockItems).WithName(nameof(CreateStockItems));

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
}
