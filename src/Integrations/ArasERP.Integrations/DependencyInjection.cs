using ArasERP.Integrations.Mengantar;
using ArasERP.Integrations.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ArasERP.Integrations;

public static class DependencyInjection
{
    public static IServiceCollection AddIntegrations(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.Configure<IntegrationsOptions>(
            configuration.GetSection(IntegrationsOptions.SectionName)
        );

        services.AddHttpClient<MengantarClient>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        return services;
    }
}
