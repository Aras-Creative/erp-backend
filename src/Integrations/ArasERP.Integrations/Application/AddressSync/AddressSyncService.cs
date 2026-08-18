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

    private readonly IKeywordSyncStateCache _syncStateCache;

    public AddressSyncService(
        IShippingProviderFactory providerFactory,
        IAddressWriter addressWriter,
        IKeywordSyncStateCache syncStateCache
    )
    {
        _providerFactory = providerFactory;
        _addressWriter = addressWriter;
        _syncStateCache = syncStateCache;
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
        var state = _syncStateCache.Get(provider.Name, keyword);
        var etag = state?.ETag;
        var result = await provider.SearchAddressesAsync(keyword, etag, cancellationToken);

        if (result.NotModified)
        {
            _syncStateCache.Set(provider.Name, keyword, etag, DateTimeOffset.UtcNow);
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

        var newEtag = !string.IsNullOrWhiteSpace(result.Etag) ? result.Etag : etag;
        _syncStateCache.Set(provider.Name, keyword, newEtag, DateTimeOffset.UtcNow);

        return new AddressSyncResult(keyword, result.Items.Count, false);
    }
}
