using ArasERP.Integrations.Abstractions;
using ArasERP.Integrations.Infrastructure.Providers.Mengantar;
using ArasERP.Integrations.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ArasERP.Integrations.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddIntegrationsInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.Configure<IntegrationsOptions>(
            configuration.GetSection(IntegrationsOptions.SectionName)
        );

        services.AddHttpClient<MengantarProvider>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        services.AddScoped<IShippingProvider, MengantarProvider>();

        return services;
    }
}
