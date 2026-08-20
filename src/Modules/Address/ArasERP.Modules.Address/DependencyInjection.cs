using ArasERP.BuildingBlocks.Application;
using ArasERP.Modules.Address.Application;
using ArasERP.Modules.Address.Application.Search;
using ArasERP.Modules.Address.Domain;
using ArasERP.Modules.AddressClient;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace ArasERP.Modules.Address;

public static class DependencyInjection
{
    public static IServiceCollection AddAddressModule(this IServiceCollection services)
    {
        services.AddScoped<IValidator<SearchAddressesQuery>, SearchAddressesQueryValidator>();

        services.AddScoped<
            IQueryHandler<SearchAddressesQuery, IReadOnlyList<Domain.Address>>,
            SearchAddressesQueryHandler
        >();

        services.AddScoped<ArasERP.Modules.AddressClient.IAddressClient, AddressClientImpl>();

        return services;
    }
}
