using ArasERP.Modules.Address.Application.Sync;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ArasERP.Modules.Address.Infrastructure.BackgroundServices;

public sealed class ProvinceSeedBackgroundService(
    ILogger<ProvinceSeedBackgroundService> logger,
    IOptions<AddressSyncOptions> options,
    IServiceProvider serviceProvider,
    AddressKeywordFetchLock fetchLock
) : BackgroundService
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

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var interval = TimeSpan.FromMinutes(options.Value.IntervalMinutes);
        var timer = new PeriodicTimer(interval);

        logger.LogInformation(
            "Province seed service started. Interval: {IntervalMinutes}m, Keywords: {Count}",
            options.Value.IntervalMinutes,
            ProvinceKeywords.Length
        );

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
                logger.LogError(ex, "Province seed cycle failed");
            }
        }
    }

    private async Task SeedAllKeywordsAsync(CancellationToken cancellationToken)
    {
        var batches = ProvinceKeywords.Chunk(options.Value.BatchSize).ToList();

        var processedCount = 0;
        var skippedCount = 0;
        var failedCount = 0;

        foreach (
            var tasks in batches.Select(batch =>
                batch.Select(keyword => SeedKeywordAsync(keyword, cancellationToken))
            )
        )
        {
            var results = await Task.WhenAll(tasks);

            processedCount += results.Count(r => r == SeedResult.Processed);
            skippedCount += results.Count(r => r == SeedResult.Skipped);
            failedCount += results.Count(r => r == SeedResult.Failed);
        }

        logger.LogInformation(
            "Province seed cycle completed. Processed: {Processed}, Skipped: {Skipped}, Failed: {Failed}",
            processedCount,
            skippedCount,
            failedCount
        );
    }

    private async Task<SeedResult> SeedKeywordAsync(
        string keyword,
        CancellationToken cancellationToken
    )
    {
        using var lockHandle = await fetchLock.TryAcquireAsync(
            keyword,
            TimeSpan.FromSeconds(2),
            cancellationToken
        );

        if (lockHandle is null)
        {
            return SeedResult.Skipped;
        }

        try
        {
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(
                cancellationToken
            );
            timeoutCts.CancelAfter(TimeSpan.FromSeconds(options.Value.TimeoutPerKeywordSeconds));

            using var scope = serviceProvider.CreateScope();
            var syncService = scope.ServiceProvider.GetRequiredService<AddressSyncerService>();

            await syncService.SyncAsync(keyword, timeoutCts.Token);

            return SeedResult.Processed;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to sync keyword '{Keyword}'", keyword);
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
