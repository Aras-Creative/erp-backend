using ArasERP.BuildingBlocks.Application;
using ArasERP.Modules.Address.Application.Abstractions;
using ArasERP.Modules.Address.Application.Options;
using ArasERP.Modules.Address.Application.Addresses.Sync;
using ArasERP.Modules.Address.Contracts.Addresses;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ValidationException = ArasERP.BuildingBlocks.Application.ValidationException;

namespace ArasERP.Modules.Address.Application.Addresses.Search;

internal sealed class SearchAddressesQueryHandler : IQueryHandler<SearchAddressesQuery, IReadOnlyList<AddressSearchResultItemDto>>
{
    private readonly IAddressRepository _repository;
    private readonly AddressSyncService _syncService;
    private readonly IKeywordSyncStateCache _syncStateCache;
    private readonly KeywordFetchLock _fetchLock;
    private readonly IOptions<AddressSyncOptions> _options;
    private readonly ILogger<SearchAddressesQueryHandler> _logger;

    public SearchAddressesQueryHandler(
        IAddressRepository repository,
        AddressSyncService syncService,
        IKeywordSyncStateCache syncStateCache,
        KeywordFetchLock fetchLock,
        IOptions<AddressSyncOptions> options,
        ILogger<SearchAddressesQueryHandler> logger)
    {
        _repository = repository;
        _syncService = syncService;
        _syncStateCache = syncStateCache;
        _fetchLock = fetchLock;
        _options = options;
        _logger = logger;
    }

    public async Task<IReadOnlyList<AddressSearchResultItemDto>> Handle(
        SearchAddressesQuery query,
        CancellationToken cancellationToken = default)
    {
        var keyword = query.Keyword.Trim();

        var localResults = await _repository.SearchAsync(keyword, query.Limit, cancellationToken);

        if (localResults.Count == 0)
        {
            _logger.LogInformation(
                "Local search empty for '{Keyword}', attempting vendor fallback", keyword);

            await TriggerFallbackAsync(keyword, _options.Value.FallbackTimeoutSeconds, cancellationToken);

            var finalResults = await _repository.SearchAsync(keyword, query.Limit, cancellationToken);

            _logger.LogInformation(
                "Fallback complete for '{Keyword}': {Count} results found",
                keyword, finalResults.Count);

            return finalResults;
        }

        var state = _syncStateCache.Get(_options.Value.Provider, keyword);
        var lastChecked = state?.LastCheckedAt;

        if (lastChecked.HasValue
            && (DateTimeOffset.UtcNow - lastChecked.Value).TotalSeconds < _options.Value.FreshnessTtlSeconds)
        {
            _logger.LogDebug(
                "Data fresh for '{Keyword}' (TTL hit, last checked {LastChecked:s}), skipping vendor",
                keyword, lastChecked.Value);
            return localResults;
        }

        _logger.LogDebug(
            "TTL expired or never checked for '{Keyword}', running freshness check", keyword);

        return await CheckFreshnessAndReturnAsync(keyword, query.Limit, localResults, cancellationToken);
    }

    private async Task<IReadOnlyList<AddressSearchResultItemDto>> CheckFreshnessAndReturnAsync(
        string keyword,
        int limit,
        IReadOnlyList<AddressSearchResultItemDto> localResults,
        CancellationToken cancellationToken)
    {
        using var lockHandle = await _fetchLock.TryAcquireAsync(
            keyword,
            TimeSpan.FromSeconds(2),
            cancellationToken);

        if (lockHandle is null)
        {
            _logger.LogDebug(
                "Freshness check skipped for '{Keyword}', sync already in-progress", keyword);
            return localResults;
        }

        try
        {
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(TimeSpan.FromSeconds(_options.Value.FreshnessCheckTimeoutSeconds));

            var result = await _syncService.SyncAsync(keyword, timeoutCts.Token);

            if (result.ProcessedCount > 0)
            {
                _logger.LogInformation(
                    "Freshness check synced {Count} new records for '{Keyword}', re-querying",
                    result.ProcessedCount, keyword);
                return await _repository.SearchAsync(keyword, limit, cancellationToken);
            }

            _logger.LogDebug(
                "Data fresh for '{Keyword}' (ETag match or no changes)", keyword);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (OperationCanceledException)
        {
            _logger.LogDebug(
                "Freshness check timed out for '{Keyword}' after {Timeout}s, returning local results",
                keyword, _options.Value.FreshnessCheckTimeoutSeconds);
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Freshness check failed for '{Keyword}', returning local results", keyword);
        }

        return localResults;
    }

    private async Task TriggerFallbackAsync(string keyword, int timeoutSeconds, CancellationToken cancellationToken)
    {
        using var lockHandle = await _fetchLock.TryAcquireAsync(
            keyword,
            TimeSpan.FromSeconds(2),
            cancellationToken);

        if (lockHandle is null)
        {
            _logger.LogInformation(
                "Fallback already in-progress for '{Keyword}', skipping", keyword);
            return;
        }

        try
        {
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(TimeSpan.FromSeconds(timeoutSeconds));

            _logger.LogInformation(
                "Calling vendor sync for '{Keyword}' (timeout: {Timeout}s)",
                keyword, timeoutSeconds);

            var result = await _syncService.SyncAsync(keyword, timeoutCts.Token);

            _logger.LogInformation(
                "Vendor sync for '{Keyword}': {Count} records synced",
                keyword, result.ProcessedCount);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning(
                "Vendor fallback timed out for '{Keyword}' after {Timeout}s",
                keyword, timeoutSeconds);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Vendor fallback failed for '{Keyword}': {Error}",
                keyword, ex.Message);
        }
    }
}
