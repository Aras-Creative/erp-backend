using ArasERP.BuildingBlocks.Application;
using ArasERP.BuildingBlocks.Presentation;
using ArasERP.Modules.Address.Contracts.Addresses;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;

namespace ArasERP.Modules.Address.Api.Endpoints;

public static class AddressesEndpoints
{
    public static IEndpointRouteBuilder MapAddressEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/addresses").WithTags("Addresses");

        group.MapGet("/search", SearchAddresses).WithName(nameof(SearchAddresses));

        return app;
    }

    private static async Task<Ok<ApiResponse<IReadOnlyList<AddressSearchResultItemDto>>>> SearchAddresses(
        HttpContext context,
        IAddressSearchFallbackService searchService,
        CancellationToken cancellationToken,
        string keyword,
        int limit = PaginationDefaults.MaxPageSize
    )
    {
        var results = await searchService.SearchAsync(keyword, limit, cancellationToken);

        return TypedResults.Ok(ApiResponse.Success(context, results));
    }
}
