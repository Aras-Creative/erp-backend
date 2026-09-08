using ArasERP.Integrations.Mengantar;
using ArasERP.Modules.Address.Application.Abstractions;
using ArasERP.Modules.Address.Domain;

namespace ArasERP.Modules.Address.Infrastructure.Providers.Mengantar;

internal sealed class MengantarAddressProvider : IAddressProvider
{
    private readonly MengantarClient _client;

    public MengantarAddressProvider(MengantarClient client)
    {
        _client = client;
    }

    public async Task<AddressProviderResult> SearchAsync(
        string keyword,
        string? etag,
        CancellationToken ct
    )
    {
        var response = await _client.SearchAddressAsync(keyword, etag, ct);

        if (response.NotModified)
        {
            return AddressProviderResult.Unchanged();
        }

        var addresses = response.Items.Select(MapToAddress).ToList();

        return new AddressProviderResult(false, response.Etag, addresses);
    }

    private static Domain.Address MapToAddress(MengantarAddressItem item)
    {
        return Domain.Address.Create(
            destinationCode: item.DestinationCode ?? string.Empty,
            originCode: item.OriginCode ?? string.Empty,
            provinceName: item.ProvinceName ?? string.Empty,
            cityName: item.CityName ?? string.Empty,
            districtName: item.DistrictName ?? string.Empty,
            subDistrictName: item.SubDistrictName ?? string.Empty,
            zipCode: item.ZipCode ?? string.Empty,
            externalId: item.Id
        );
    }
}
