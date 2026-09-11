using ArasERP.BuildingBlocks.Application;
using ArasERP.BuildingBlocks.Presentation;
using ArasERP.Modules.Inventory.Api.Response;
using ArasERP.Modules.Inventory.Application.Warehouses.Create;
using ArasERP.Modules.Inventory.Application.Warehouses.Delete;
using ArasERP.Modules.Inventory.Application.Warehouses.GetById;
using ArasERP.Modules.Inventory.Application.Warehouses.GetOptions;
using ArasERP.Modules.Inventory.Application.Warehouses.List;
using ArasERP.Modules.Inventory.Application.Warehouses.Update;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace ArasERP.Modules.Inventory.Api.Endpoints;

public static class WarehousesEndpoints
{
    public static IEndpointRouteBuilder MapWarehouseEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/warehouses").WithTags("Warehouses");

        group.MapGet("/options", GetWarehouseOptions).WithName(nameof(GetWarehouseOptions));

        group.MapGet("/", ListWarehouses).WithName(nameof(ListWarehouses));

        group.MapGet("/{id:guid}", GetWarehouseById).WithName(nameof(GetWarehouseById));

        group.MapPost("/", CreateWarehouse).WithName(nameof(CreateWarehouse));

        group.MapPut("/{id:guid}", UpdateWarehouse).WithName(nameof(UpdateWarehouse));

        group.MapDelete("/{id:guid}", DeleteWarehouse).WithName(nameof(DeleteWarehouse));

        return app;
    }

    private static async Task<Ok<ApiResponse<List<WarehouseOptionResponse>>>> GetWarehouseOptions(
        HttpContext context,
        IQueryHandler<GetWarehouseOptionsQuery, IReadOnlyList<WarehouseOptionDto>> handler,
        CancellationToken cancellationToken
    )
    {
        var options = await handler.Handle(new GetWarehouseOptionsQuery(), cancellationToken);

        return TypedResults.Ok(
            ApiResponse.Success(context, options.Select(o => o.ToResponse()).ToList())
        );
    }

    private static async Task<Ok<ApiResponse<PagedList<WarehouseResponse>>>> ListWarehouses(
        HttpContext context,
        IQueryHandler<ListWarehousesQuery, PagedList<WarehouseListItemDto>> handler,
        CancellationToken cancellationToken,
        [FromQuery] int page = PaginationDefaults.Page,
        [FromQuery] int pageSize = PaginationDefaults.PageSize
    )
    {
        page = Math.Max(page, PaginationDefaults.Page);
        pageSize = Math.Clamp(pageSize, 1, PaginationDefaults.MaxPageSize);

        var paged = await handler.Handle(
            new ListWarehousesQuery(page, pageSize),
            cancellationToken
        );

        return TypedResults.Ok(ApiResponse.Success(context, paged.ToResponse()));
    }

    private static async Task<
        Results<Ok<ApiResponse<WarehouseResponse>>, NotFound<ApiResponse<object>>>
    > GetWarehouseById(
        HttpContext context,
        Guid id,
        IQueryHandler<GetWarehouseByIdQuery, WarehouseDetailDto?> handler,
        CancellationToken cancellationToken
    )
    {
        var warehouse = await handler.Handle(new GetWarehouseByIdQuery(id), cancellationToken);

        return warehouse is null
            ? TypedResults.NotFound(
                ApiResponse.Fail<object>(
                    context,
                    StatusCodes.Status404NotFound,
                    $"Warehouse with id '{id}' was not found."
                )
            )
            : TypedResults.Ok(ApiResponse.Success(context, warehouse.ToResponse()));
    }

    private static async Task<Created> CreateWarehouse(
        CreateWarehouseCommand command,
        IMediator mediator,
        CancellationToken cancellationToken
    )
    {
        await mediator.SendAsync<CreateWarehouseCommand>(command, cancellationToken);
        return TypedResults.Created();
    }

    private static async Task<NoContent> UpdateWarehouse(
        Guid id,
        UpdateWarehouseCommand command,
        IMediator mediator,
        CancellationToken cancellationToken
    )
    {
        command.WarehouseId = id.ToString();
        await mediator.SendAsync<UpdateWarehouseCommand>(command, cancellationToken);
        return TypedResults.NoContent();
    }

    private static async Task<NoContent> DeleteWarehouse(
        Guid id,
        ICommandHandler<DeleteWarehouseCommand> handler,
        CancellationToken cancellationToken
    )
    {
        var command = new DeleteWarehouseCommand { WarehouseId = id.ToString() };
        await handler.Handle(command, cancellationToken);
        return TypedResults.NoContent();
    }
}
