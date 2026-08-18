using ArasERP.Modules.Address.Application;
using ArasERP.Modules.Address.Application.Addresses.Sync;
using ArasERP.Modules.Address.Application.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ArasERP.Modules.Address.Infrastructure.BackgroundServices;

public sealed class ProvinceSeedBackgroundService : BackgroundService
{
    private static readonly string[] ProvinceKeywords =
    [
        "aceh",
        "sumatera utara",
        "sumatera barat",
        "riau",
        "kepulauan riau",
        "jambi",
        "bengkulu",
        "sumatera selatan",
        "kepulauan bangka belitung",
        "lampung",
        "dki jakarta",
        "jawa barat",
        "banten",
        "jawa tengah",
        "di yogyakarta",
        "jawa timur",
        "bali",
        "nusa tenggara barat",
        "nusa tenggara timur",
        "kalimantan barat",
        "kalimantan tengah",
        "kalimantan selatan",
        "kalimantan timur",
        "kalimantan utara",
        "sulawesi utara",
        "gorontalo",
        "sulawesi tengah",
        "sulawesi barat",
        "sulawesi selatan",
        "sulawesi tenggara",
        "maluku",
        "maluku utara",
        "papua",
        "papua barat",
        "papua selatan",
        "papua tengah",
        "papua pegunungan",
        "papua barat daya",
    ];

    private readonly ILogger<ProvinceSeedBackgroundService> _logger;
    private readonly IOptions<AddressSyncOptions> _options;
    private readonly IServiceProvider _serviceProvider;
    private readonly KeywordFetchLock _fetchLock;

    public ProvinceSeedBackgroundService(
        ILogger<ProvinceSeedBackgroundService> logger,
        IOptions<AddressSyncOptions> options,
        IServiceProvider serviceProvider,
        KeywordFetchLock fetchLock)
    {
        _logger = logger;
        _options = options;
        _serviceProvider = serviceProvider;
        _fetchLock = fetchLock;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var interval = TimeSpan.FromMinutes(_options.Value.IntervalMinutes);
        var timer = new PeriodicTimer(interval);

        _logger.LogInformation(
            "Province seed service started. Interval: {IntervalMinutes}m, Keywords: {Count}",
            _options.Value.IntervalMinutes,
            ProvinceKeywords.Length);

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await SeedAllKeywordsAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Province seed cycle failed");
            }
        }
    }

    private async Task SeedAllKeywordsAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting province seed cycle for {Count} keywords", ProvinceKeywords.Length);

        var batches = ProvinceKeywords
            .Chunk(_options.Value.BatchSize)
            .ToList();

        var processedCount = 0;
        var skippedCount = 0;
        var failedCount = 0;

        foreach (var batch in batches)
        {
            var tasks = batch.Select(keyword => SeedKeywordAsync(keyword, cancellationToken));
            var results = await Task.WhenAll(tasks);

            processedCount += results.Count(r => r == SeedResult.Processed);
            skippedCount += results.Count(r => r == SeedResult.Skipped);
            failedCount += results.Count(r => r == SeedResult.Failed);
        }

        _logger.LogInformation(
            "Province seed cycle completed. Processed: {Processed}, Skipped: {Skipped}, Failed: {Failed}",
            processedCount,
            skippedCount,
            failedCount);
    }

    private async Task<SeedResult> SeedKeywordAsync(
        string keyword,
        CancellationToken cancellationToken)
    {
        using var lockHandle = await _fetchLock.TryAcquireAsync(
            keyword,
            TimeSpan.FromSeconds(2),
            cancellationToken);

        if (lockHandle is null)
        {
            _logger.LogDebug("Skipping keyword '{Keyword}' — already in progress", keyword);
            return SeedResult.Skipped;
        }

        try
        {
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(TimeSpan.FromSeconds(_options.Value.TimeoutPerKeywordSeconds));

            using var scope = _serviceProvider.CreateScope();
            var syncService = scope.ServiceProvider.GetRequiredService<AddressSyncService>();

            var result = await syncService.SyncAsync(keyword, timeoutCts.Token);

            if (result.NotModified)
            {
                _logger.LogDebug("Keyword '{Keyword}' — not modified (ETag match)", keyword);
                return SeedResult.Processed;
            }

            _logger.LogInformation(
                "Keyword '{Keyword}' — synced {Count} records",
                keyword,
                result.ProcessedCount);

            return SeedResult.Processed;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to sync keyword '{Keyword}'", keyword);
            return SeedResult.Failed;
        }
    }

    private enum SeedResult
    {
        Processed,
        Skipped,
        Failed,
    }
}
