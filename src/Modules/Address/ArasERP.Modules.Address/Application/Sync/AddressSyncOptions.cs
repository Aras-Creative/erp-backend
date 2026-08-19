namespace ArasERP.Modules.Address.Application.Sync;

public sealed class AddressSyncOptions
{
    public int IntervalMinutes { get; init; } = 360;

    public string Provider { get; init; } = "mengantar";

    public int BatchSize { get; init; } = 3;

    public int TimeoutPerKeywordSeconds { get; init; } = 15;

    public int FallbackTimeoutSeconds { get; init; } = 15;

    public int FreshnessCheckTimeoutSeconds { get; init; } = 3;

    public int FreshnessTtlSeconds { get; init; } = 3600;
}
