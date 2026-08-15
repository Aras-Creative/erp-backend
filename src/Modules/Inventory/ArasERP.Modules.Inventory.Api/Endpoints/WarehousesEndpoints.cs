using ArasERP.BuildingBlocks.Application;
using ArasERP.Modules.Inventory.Application.Warehouses.Create;
using ArasERP.Modules.Inventory.Application.Warehouses.Delete;
using ArasERP.Modules.Inventory.Application.Warehouses.Update;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace ArasERP.Modules.Inventory.Api.Endpoints;

public static class WarehousesEndpoints
{
    public static IEndpointRouteBuilder MapWarehouseEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/warehouses").WithTags("Warehouses");

        group.MapPost("/", CreateWarehouse).WithName(nameof(CreateWarehouse));

        group.MapPut("/{id:guid}", UpdateWarehouse).WithName(nameof(UpdateWarehouse));

        group.MapDelete("/{id:guid}", DeleteWarehouse).WithName(nameof(DeleteWarehouse));

        return app;
    }

    private static async Task<IResult> CreateWarehouse(
        CreateWarehouseCommand command,
        ICommandHandler<CreateWarehouseCommand> handler,
        CancellationToken cancellationToken
    )
    {
        await handler.Handle(command, cancellationToken);
        return Results.Ok();
    }

    private static async Task<IResult> UpdateWarehouse(
        Guid id,
        UpdateWarehouseCommand command,
        ICommandHandler<UpdateWarehouseCommand> handler,
        CancellationToken cancellationToken
    )
    {
        command.WarehouseId = id.ToString();
        await handler.Handle(command, cancellationToken);
        return Results.Ok();
    }

    private static async Task<IResult> DeleteWarehouse(
        Guid id,
        ICommandHandler<DeleteWarehouseCommand> handler,
        CancellationToken cancellationToken
    )
    {
        var command = new DeleteWarehouseCommand { WarehouseId = id.ToString() };
        await handler.Handle(command, cancellationToken);
        return Results.Ok();
    }
}
