using ArasERP.BuildingBlocks.Application;
using ArasERP.Modules.Address.Application.Abstractions;
using ArasERP.Modules.Address.Application.Sync;
using ArasERP.Modules.Address.Domain;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ArasERP.Modules.Address.Application.Addresses.Search;

internal sealed class SearchAddressesQueryHandler(
    IAddressRepository repository,
    AddressSyncerService syncService,
    IKeywordSyncStateCache syncStateCache,
    AddressKeywordFetchLock fetchLock,
    IOptions<AddressSyncOptions> options,
    ILogger<SearchAddressesQueryHandler> logger)
    : IQueryHandler<SearchAddressesQuery, IReadOnlyList<Domain.Address>>
{
    public async Task<IReadOnlyList<Domain.Address>> Handle(
        SearchAddressesQuery query,
        CancellationToken cancellationToken = default)
    {
        var keyword = query.Keyword.Trim();

        var localResults = await repository.SearchAsync(keyword, query.Limit, cancellationToken);

        if (localResults.Count == 0)
        {
            logger.LogInformation("Local empty for '{Keyword}', triggering fallback", keyword);
            await TriggerFallbackAsync(keyword, options.Value.FallbackTimeoutSeconds, cancellationToken);
            return await repository.SearchAsync(keyword, query.Limit, cancellationToken);
        }

        var state = syncStateCache.Get(options.Value.Provider, keyword);
        var lastChecked = state?.LastCheckedAt;

        if (lastChecked.HasValue
            && (DateTimeOffset.UtcNow - lastChecked.Value).TotalSeconds < options.Value.FreshnessTtlSeconds)
        {
            return localResults;
        }

        return await CheckFreshnessAndReturnAsync(keyword, query.Limit, localResults, cancellationToken);
    }

    private async Task<IReadOnlyList<Domain.Address>> CheckFreshnessAndReturnAsync(
        string keyword,
        int limit,
        IReadOnlyList<Domain.Address> localResults,
        CancellationToken cancellationToken)
    {
        using var lockHandle = await fetchLock.TryAcquireAsync(
            keyword,
            TimeSpan.FromSeconds(2),
            cancellationToken);

        if (lockHandle is null)
        {
            return localResults;
        }

        try
        {
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(TimeSpan.FromSeconds(options.Value.FreshnessCheckTimeoutSeconds));

            var result = await syncService.SyncAsync(keyword, timeoutCts.Token);

            if (result.ProcessedCount > 0)
            {
                logger.LogInformation("Freshness check synced {Count} new records for '{Keyword}'", result.ProcessedCount, keyword);
                return await repository.SearchAsync(keyword, limit, cancellationToken);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (OperationCanceledException)
        {
            logger.LogWarning("Freshness check timed out for '{Keyword}' after {Timeout}s", keyword, options.Value.FreshnessCheckTimeoutSeconds);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Freshness check failed for '{Keyword}'", keyword);
        }

        return localResults;
    }

    private async Task TriggerFallbackAsync(string keyword, int timeoutSeconds, CancellationToken cancellationToken)
    {
        using var lockHandle = await fetchLock.TryAcquireAsync(
            keyword,
            TimeSpan.FromSeconds(2),
            cancellationToken);

        if (lockHandle is null)
        {
            return;
        }

        try
        {
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(TimeSpan.FromSeconds(timeoutSeconds));

            await syncService.SyncAsync(keyword, timeoutCts.Token);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (OperationCanceledException)
        {
            logger.LogWarning("Fallback timed out for '{Keyword}' after {Timeout}s", keyword, timeoutSeconds);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Fallback failed for '{Keyword}'", keyword);
        }
    }
}
