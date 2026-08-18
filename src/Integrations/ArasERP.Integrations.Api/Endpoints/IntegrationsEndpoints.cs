using ArasERP.BuildingBlocks.Presentation;
using ArasERP.Integrations.Abstractions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;

namespace ArasERP.Integrations.Api.Endpoints;

public static class IntegrationsEndpoints
{
    public static IEndpointRouteBuilder MapIntegrationsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/integrations").WithTags("Integrations");

        group.MapGet("/providers", ListProviders).WithName(nameof(ListProviders));

        return app;
    }

    private static Ok<ApiResponse<List<string>>> ListProviders(
        HttpContext context,
        IShippingProviderFactory providerFactory
    )
    {
        var providers = providerFactory.GetAll().Select(p => p.Name).ToList();

        return TypedResults.Ok(ApiResponse.Success(context, providers));
    }
}
