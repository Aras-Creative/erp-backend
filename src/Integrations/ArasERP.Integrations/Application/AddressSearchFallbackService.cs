using ArasERP.Integrations.Application.AddressSync;
using ArasERP.Integrations.Options;
using ArasERP.Modules.Address.Application.Abstractions;
using ArasERP.Modules.Address.Contracts.Addresses;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ArasERP.Integrations.Application;

public sealed class AddressSearchFallbackService : IAddressSearchFallbackService
{
    private readonly IAddressRepository _addressRepository;
    private readonly AddressSyncService _syncService;
    private readonly KeywordFetchLock _fetchLock;
    private readonly IOptions<AddressSyncOptions> _options;
    private readonly ILogger<AddressSearchFallbackService> _logger;

    public AddressSearchFallbackService(
        IAddressRepository addressRepository,
        AddressSyncService syncService,
        KeywordFetchLock fetchLock,
        IOptions<AddressSyncOptions> options,
        ILogger<AddressSearchFallbackService> logger
    )
    {
        _addressRepository = addressRepository;
        _syncService = syncService;
        _fetchLock = fetchLock;
        _options = options;
        _logger = logger;
    }

    public async Task<IReadOnlyList<AddressSearchResultItemDto>> SearchAsync(
        string keyword,
        int limit,
        CancellationToken cancellationToken = default)
    {
        keyword = keyword.Trim();

        var localResults = await _addressRepository.SearchAsync(keyword, limit, cancellationToken);

        if (localResults.Count > 0)
        {
            return localResults;
        }

        _logger.LogInformation(
            "Local search empty for '{Keyword}', attempting vendor fallback",
            keyword
        );

        await TriggerFallbackAsync(keyword, cancellationToken);

        var finalResults = await _addressRepository.SearchAsync(keyword, limit, cancellationToken);

        _logger.LogInformation(
            "Fallback complete for '{Keyword}': {Count} results found",
            keyword,
            finalResults.Count
        );

        return finalResults;
    }

    private async Task TriggerFallbackAsync(string keyword, CancellationToken cancellationToken)
    {
        using var lockHandle = await _fetchLock.TryAcquireAsync(
            keyword,
            TimeSpan.FromSeconds(2),
            cancellationToken
        );

        if (lockHandle is null)
        {
            _logger.LogInformation(
                "Fallback already in-progress for '{Keyword}', skipping",
                keyword
            );
            return;
        }

        try
        {
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(TimeSpan.FromSeconds(_options.Value.FallbackTimeoutSeconds));

            _logger.LogInformation(
                "Calling vendor sync for '{Keyword}' (timeout: {Timeout}s)",
                keyword,
                _options.Value.FallbackTimeoutSeconds
            );

            var result = await _syncService.SyncAsync(
                _options.Value.Provider,
                keyword,
                timeoutCts.Token
            );

            _logger.LogInformation(
                "Vendor sync for '{Keyword}': {Count} records synced",
                keyword,
                result.ProcessedCount
            );
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning(
                "Vendor fallback timed out for '{Keyword}' after {Timeout}s",
                keyword,
                _options.Value.FallbackTimeoutSeconds
            );
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Vendor fallback failed for '{Keyword}': {Error}",
                keyword, ex.Message);
        }
    }
}
