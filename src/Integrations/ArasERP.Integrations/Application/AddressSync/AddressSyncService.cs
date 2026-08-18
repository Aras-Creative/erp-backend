using ArasERP.BuildingBlocks.Application;
using ArasERP.Integrations.Abstractions;
using ArasERP.Modules.Address.Contracts.Addresses;

namespace ArasERP.Integrations.Application.AddressSync;

public sealed record AddressSyncResult(string Keyword, int ProcessedCount, bool NotModified);

public sealed class AddressSyncService
{
    public const string DefaultProvider = "mengantar";

    private readonly IShippingProviderFactory _providerFactory;

    private readonly IAddressWriter _addressWriter;

    private readonly IKeywordEtagCache _etagCache;

    public AddressSyncService(
        IShippingProviderFactory providerFactory,
        IAddressWriter addressWriter,
        IKeywordEtagCache etagCache
    )
    {
        _providerFactory = providerFactory;
        _addressWriter = addressWriter;
        _etagCache = etagCache;
    }

    public async Task<AddressSyncResult> SyncAsync(
        string providerName,
        string keyword,
        CancellationToken cancellationToken = default
    )
    {
        keyword = keyword.Trim();
        if (string.IsNullOrWhiteSpace(keyword))
        {
            throw new ValidationException("A keyword is required to sync addresses.");
        }

        var provider = _providerFactory.Get(providerName);
        var etag = _etagCache.Get(provider.Name, keyword);
        var result = await provider.SearchAddressesAsync(keyword, etag, cancellationToken);

        if (result.NotModified)
        {
            return new AddressSyncResult(keyword, 0, true);
        }

        var items = result.Items
            .Select(
                item =>
                    new AddressSyncItem(
                        item.ProviderId,
                        item.OriginCode,
                        item.DestinationCode,
                        item.ProvinceName,
                        item.CityName,
                        item.DistrictName,
                        item.SubDistrictName,
                        item.ZipCode
                    )
            )
            .ToList();

        await _addressWriter.UpsertAsync(items, cancellationToken);

        if (!string.IsNullOrWhiteSpace(result.Etag))
        {
            _etagCache.Set(provider.Name, keyword, result.Etag);
        }

        return new AddressSyncResult(keyword, result.Items.Count, false);
    }
}
