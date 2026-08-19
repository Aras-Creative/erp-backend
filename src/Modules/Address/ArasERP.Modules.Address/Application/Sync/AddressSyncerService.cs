using ArasERP.BuildingBlocks.Application;
using ArasERP.Modules.Address.Application.Abstractions;
using Microsoft.Extensions.Options;

namespace ArasERP.Modules.Address.Application.Sync;

public class AddressSyncerService(
    IAddressProvider provider,
    IAddressRepository repository,
    IKeywordSyncStateCache syncStateCache,
    IOptions<AddressSyncOptions> options)
{
    public virtual async Task<AddressSyncResult> SyncAsync(
        string keyword,
        CancellationToken cancellationToken = default)
    {
        keyword = keyword.Trim();
        if (string.IsNullOrWhiteSpace(keyword))
        {
            throw new ValidationException("A keyword is required to sync addresses.");
        }

        var state = syncStateCache.Get(options.Value.Provider, keyword);
        var etag = state?.ETag;

        var providerResult = await provider.SearchAsync(keyword, etag, cancellationToken);

        if (providerResult.NotModified)
        {
            syncStateCache.Set(options.Value.Provider, keyword, etag, DateTimeOffset.UtcNow);
            return new AddressSyncResult(keyword, 0, true);
        }

        if (providerResult.Items.Count > 0)
        {
            await repository.UpsertRangeAsync(providerResult.Items, cancellationToken);
        }

        var newEtag = !string.IsNullOrWhiteSpace(providerResult.Etag) ? providerResult.Etag : etag;
        syncStateCache.Set(options.Value.Provider, keyword, newEtag, DateTimeOffset.UtcNow);

        return new AddressSyncResult(keyword, providerResult.Items.Count, false);
    }
}
