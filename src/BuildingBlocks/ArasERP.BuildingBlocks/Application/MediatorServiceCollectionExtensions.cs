using Microsoft.Extensions.DependencyInjection;

namespace ArasERP.BuildingBlocks.Application;

public static class MediatorServiceCollectionExtensions
{
    public static IServiceCollection AddMediator(this IServiceCollection services)
    {
        services.AddScoped<IMediator, Mediator>();

        return services;
    }
}
