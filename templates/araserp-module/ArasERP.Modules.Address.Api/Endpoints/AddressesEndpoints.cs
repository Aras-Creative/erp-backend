using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace ArasERP.Modules.Address.Api.Endpoints;

public static class AddressesEndpoints
{
    public static IEndpointRouteBuilder MapAddressEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGroup("api/addresses").WithTags("Addresses");

        return app;
    }
}
