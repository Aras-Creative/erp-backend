using ArasERP.BuildingBlocks.Application;
using ArasERP.Modules.Inventory.Application.Warehouses.Create;
using ArasERP.Modules.Inventory.Application.Warehouses.Delete;
using ArasERP.Modules.Inventory.Application.Warehouses.GetById;
using ArasERP.Modules.Inventory.Application.Warehouses.GetOptions;
using ArasERP.Modules.Inventory.Application.Warehouses.List;
using ArasERP.Modules.Inventory.Application.Warehouses.Update;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace ArasERP.Modules.Inventory;

public static class DependencyInjection
{
    public static IServiceCollection AddInventoryModule(this IServiceCollection services)
    {
        services.AddScoped<IValidator<CreateWarehouseCommand>, CreateWarehouseCommandValidator>();
        services.AddScoped<
            ICommandHandler<CreateWarehouseCommand>,
            CreateWarehouseCommandHandler
        >();

        services.AddScoped<IValidator<UpdateWarehouseCommand>, UpdateWarehouseCommandValidator>();
        services.AddScoped<
            ICommandHandler<UpdateWarehouseCommand>,
            UpdateWarehouseCommandHandler
        >();

        services.AddScoped<IValidator<DeleteWarehouseCommand>, DeleteWarehouseCommandValidator>();
        services.AddScoped<
            ICommandHandler<DeleteWarehouseCommand>,
            DeleteWarehouseCommandHandler
        >();

        services.AddScoped<
            IQueryHandler<GetWarehouseOptionsQuery, IReadOnlyList<WarehouseOptionDto>>,
            GetWarehouseOptionsQueryHandler
        >();

        services.AddScoped<
            IQueryHandler<ListWarehousesQuery, PagedList<WarehouseListItemDto>>,
            ListWarehousesQueryHandler
        >();

        services.AddScoped<
            IQueryHandler<GetWarehouseByIdQuery, WarehouseDetailDto?>,
            GetWarehouseByIdQueryHandler
        >();

        return services;
    }
}
