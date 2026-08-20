using ArasERP.BuildingBlocks.Application;
using ArasERP.BuildingBlocks.Presentation;
using ArasERP.Modules.Address.Api.Dtos;
using ArasERP.Modules.Address.Application.Search;
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
        IMediator mediator,
        CancellationToken cancellationToken,
        string keyword,
        int limit = PaginationDefaults.MaxPageSize)
    {
        var query = new SearchAddressesQuery { Keyword = keyword, Limit = limit };
        var results = await mediator.SendAsync<SearchAddressesQuery, IReadOnlyList<Domain.Address>>(query, cancellationToken);

        IReadOnlyList<AddressSearchResultItemDto> dto =
        [
            .. results.Select(a => new AddressSearchResultItemDto
            {
                AddressId = a.Id.Value,
                ExternalId = a.ExternalId,
                DestinationCode = a.DestinationCode,
                OriginCode = a.OriginCode,
                ProvinceName = a.ProvinceName,
                CityName = a.CityName,
                DistrictName = a.DistrictName,
                SubDistrictName = a.SubDistrictName,
                ZipCode = a.ZipCode,
            })
        ];

        return TypedResults.Ok(ApiResponse.Success(context, dto));
    }
}
