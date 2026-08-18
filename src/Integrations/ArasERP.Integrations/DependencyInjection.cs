using ArasERP.Integrations.Abstractions;
using ArasERP.Integrations.Application;
using ArasERP.Integrations.Application.AddressSync;
using ArasERP.Integrations.Options;
using ArasERP.Modules.Address.Contracts.Addresses;
using Microsoft.Extensions.DependencyInjection;

namespace ArasERP.Integrations;

public static class DependencyInjection
{
    public static IServiceCollection AddIntegrations(this IServiceCollection services)
    {
        services.AddSingleton<IKeywordSyncStateCache, InMemoryKeywordSyncStateCache>();
        services.AddSingleton<KeywordFetchLock>();

        services.AddScoped<IShippingProviderFactory, ShippingProviderFactory>();
        services.AddScoped<AddressSyncService>();
        services.AddScoped<IAddressSearchFallbackService, AddressSearchFallbackService>();

        services.AddHostedService<ProvinceSeedBackgroundService>();

        services.AddOptions<AddressSyncOptions>()
            .BindConfiguration("Integrations:AddressSync")
            .ValidateDataAnnotations()
            .ValidateOnStart();

        return services;
    }
}
