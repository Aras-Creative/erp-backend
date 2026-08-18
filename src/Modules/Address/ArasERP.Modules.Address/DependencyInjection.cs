using ArasERP.BuildingBlocks.Application;
using ArasERP.Modules.Address.Application.Addresses;
using ArasERP.Modules.Address.Application.Addresses.Search;
using ArasERP.Modules.Address.Application.Addresses.Upsert;
using ArasERP.Modules.Address.Contracts.Addresses;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace ArasERP.Modules.Address;

public static class DependencyInjection
{
    public static IServiceCollection AddAddressModule(this IServiceCollection services)
    {
        services.AddScoped<IValidator<SearchAddressesQuery>, SearchAddressesQueryValidator>();

        services.AddScoped<
            IQueryHandler<SearchAddressesQuery, IReadOnlyList<AddressSearchResultItemDto>>,
            SearchAddressesQueryHandler
        >();

        services.AddScoped<
            ICommandHandler<UpsertAddressesCommand>,
            UpsertAddressesCommandHandler
        >();

        services.AddScoped<IAddressWriter, AddressWriter>();

        return services;
    }
}
