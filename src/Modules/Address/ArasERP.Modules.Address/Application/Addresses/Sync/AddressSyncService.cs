using ArasERP.BuildingBlocks.Application;
using ArasERP.Modules.Address.Application.Abstractions;
using ArasERP.Modules.Address.Application.Options;
using ArasERP.Modules.Address.Domain;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ValidationException = ArasERP.BuildingBlocks.Application.ValidationException;

namespace ArasERP.Modules.Address.Application.Addresses.Sync;

public sealed record AddressSyncResult(string Keyword, int ProcessedCount, bool NotModified);

public class AddressSyncService
{
    private readonly IAddressProvider _provider;
    private readonly IAddressRepository _repository;
    private readonly IKeywordSyncStateCache _syncStateCache;
    private readonly IOptions<AddressSyncOptions> _options;
    private readonly ILogger<AddressSyncService> _logger;

    public AddressSyncService(
        IAddressProvider provider,
        IAddressRepository repository,
        IKeywordSyncStateCache syncStateCache,
        IOptions<AddressSyncOptions> options,
        ILogger<AddressSyncService> logger)
    {
        _provider = provider;
        _repository = repository;
        _syncStateCache = syncStateCache;
        _options = options;
        _logger = logger;
    }

    public virtual async Task<AddressSyncResult> SyncAsync(
        string keyword,
        CancellationToken cancellationToken = default)
    {
        keyword = keyword.Trim();
        if (string.IsNullOrWhiteSpace(keyword))
        {
            throw new ValidationException("A keyword is required to sync addresses.");
        }

        var state = _syncStateCache.Get(_options.Value.Provider, keyword);
        var etag = state?.ETag;

        var providerResult = await _provider.SearchAsync(keyword, etag, cancellationToken);

        if (providerResult.NotModified)
        {
            _syncStateCache.Set(_options.Value.Provider, keyword, etag, DateTimeOffset.UtcNow);
            return new AddressSyncResult(keyword, 0, true);
        }

        if (providerResult.Items.Count > 0)
        {
            await _repository.UpsertRangeAsync(providerResult.Items, cancellationToken);
        }

        var newEtag = !string.IsNullOrWhiteSpace(providerResult.Etag) ? providerResult.Etag : etag;
        _syncStateCache.Set(_options.Value.Provider, keyword, newEtag, DateTimeOffset.UtcNow);

        return new AddressSyncResult(keyword, providerResult.Items.Count, false);
    }
}
