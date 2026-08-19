using Microsoft.Extensions.DependencyInjection;

namespace ArasERP.Modules.Address.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddAddressInfrastructure(
        this IServiceCollection services,
        string connectionString
    )
    {
        return services;
    }
}
