using ArasERP.BuildingBlocks.Application;
using ArasERP.Modules.Inventory.Api.Request;
using ArasERP.Modules.Inventory.Application.Batches.Create;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;

namespace ArasERP.Modules.Inventory.Api.Endpoints;

public static class BatchEndpoints
{
    public static IEndpointRouteBuilder MapBatchEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var batches = endpoints.MapGroup("api/batches").WithTags("Batches");

        batches.MapPost("/", CreateBatch).WithName(nameof(CreateBatch));

        return endpoints;
    }

    private static async Task<Created> CreateBatch(
        CreateBatchRequest request,
        IMediator mediator,
        CancellationToken cancellationToken
    )
    {
        var command = request.ToCommand(recordedBy: Guid.Empty);
        await mediator.SendAsync<CreateBatchCommand>(command, cancellationToken);
        return TypedResults.Created();
    }
}
