using ArasERP.BuildingBlocks.Application;
using ArasERP.Modules.Inventory.Application.Batches.Receive;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;

namespace ArasERP.Modules.Inventory.Api.Batches.Endpoints;

public static class BatchEndpoints
{
    public static IEndpointRouteBuilder MapBatchEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var batches = endpoints.MapGroup("api/batches").WithTags("Batches");

        batches.MapPost("/", ReceiveBatch).WithName(nameof(ReceiveBatch));

        return endpoints;
    }

    private static async Task<Created> ReceiveBatch(
        ReceiveBatchCommand command,
        IMediator mediator,
        CancellationToken cancellationToken
    )
    {
        await mediator.SendAsync<ReceiveBatchCommand>(command, cancellationToken);
        return TypedResults.Created();
    }
}
