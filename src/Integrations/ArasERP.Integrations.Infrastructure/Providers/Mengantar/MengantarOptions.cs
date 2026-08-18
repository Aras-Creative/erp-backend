namespace ArasERP.Integrations.Infrastructure.Providers.Mengantar;

public sealed class MengantarOptions
{
    public const string ProviderName = "mengantar";

    public const string DefaultBaseUrl = "https://sandbox.mengantar.com";

    public string BaseUrl { get; init; } = DefaultBaseUrl;

    public string? ApiKey { get; init; }
}
