using ArasERP.Integrations;
using ArasERP.Integrations.Mengantar;
using ArasERP.Integrations.Options;
using ArasERP.Modules.Address.Application;
using ArasERP.Modules.Address.Application.Abstractions;
using ArasERP.Modules.Address.Application.Addresses.Sync;
using ArasERP.Modules.Address.Application.Options;
using ArasERP.Modules.Address.Infrastructure.BackgroundServices;
using ArasERP.Modules.Address.Infrastructure.Persistence;
using ArasERP.Modules.Address.Infrastructure.Persistence.Repositories;
using ArasERP.Modules.Address.Infrastructure.Providers.Mengantar;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ArasERP.Modules.Address.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddAddressInfrastructure(
        this IServiceCollection services,
        string connectionString,
        IConfiguration configuration)
    {
        services.AddDbContext<AddressDbContext>(options => options.UseNpgsql(connectionString));

        services.AddScoped<IAddressRepository, AddressRepository>();

        // MengantarClient (SDK)
        services.Configure<IntegrationsOptions>(
            configuration.GetSection(IntegrationsOptions.SectionName));
        services.AddHttpClient<MengantarClient>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        // Address module internals
        services.AddSingleton<IKeywordSyncStateCache, InMemoryKeywordSyncStateCache>();
        services.AddSingleton<KeywordFetchLock>();
        services.AddScoped<IAddressProvider, MengantarAddressProvider>();
        services.AddScoped<AddressSyncService>();
        services.AddHostedService<ProvinceSeedBackgroundService>();

        services.AddOptions<AddressSyncOptions>()
            .BindConfiguration("AddressSync")
            .ValidateDataAnnotations()
            .ValidateOnStart();

        return services;
    }
}
