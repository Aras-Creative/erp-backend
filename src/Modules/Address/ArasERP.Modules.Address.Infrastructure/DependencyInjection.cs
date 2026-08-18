using ArasERP.Modules.Address.Application.Abstractions;
using ArasERP.Modules.Address.Infrastructure.Persistence;
using ArasERP.Modules.Address.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ArasERP.Modules.Address.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddAddressInfrastructure(
        this IServiceCollection services,
        string connectionString
    )
    {
        services.AddDbContext<AddressDbContext>(options => options.UseNpgsql(connectionString));

        services.AddScoped<IAddressRepository, AddressRepository>();

        return services;
    }
}
