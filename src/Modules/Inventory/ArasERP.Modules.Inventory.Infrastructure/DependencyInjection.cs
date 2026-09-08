using ArasERP.Modules.Inventory.Application.Abstractions;
using ArasERP.Modules.Inventory.Infrastructure.Persistence;
using ArasERP.Modules.Inventory.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ArasERP.Modules.Inventory.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInventoryInfrastructure(
        this IServiceCollection services,
        string connectionString
    )
    {
        services.AddDbContext<InventoryDbContext>(options => options.UseNpgsql(connectionString));

        services.AddScoped<IWarehouseRepository, WarehouseRepository>();
        services.AddScoped<IStockItemRepository, StockItemRepository>();

        return services;
    }
}
