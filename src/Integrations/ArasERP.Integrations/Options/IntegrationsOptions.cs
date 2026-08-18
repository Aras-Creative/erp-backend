namespace ArasERP.Integrations.Options;

public sealed class IntegrationsOptions
{
    public const string SectionName = "Integrations";

    public Dictionary<string, ProviderOptions> Providers { get; init; } = [];
}

public sealed class ProviderOptions
{
    public string? BaseUrl { get; init; }

    public string? ApiKey { get; init; }
}
