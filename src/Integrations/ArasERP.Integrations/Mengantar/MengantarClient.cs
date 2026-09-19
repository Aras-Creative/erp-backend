using System.Net;
using System.Net.Http.Json;
using ArasERP.Integrations.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ArasERP.Integrations.Mengantar;

public sealed class MengantarClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<MengantarClient> _logger;
    private readonly string _baseUrl;
    private readonly string _apiKey;

    public MengantarClient(
        HttpClient httpClient,
        IOptions<IntegrationsOptions> options,
        ILogger<MengantarClient> logger
    )
    {
        _httpClient = httpClient;
        _logger = logger;

        var providerOptions =
            options.Value.Providers?.GetValueOrDefault(MengantarOptions.ProviderName)
            ?? new ProviderOptions();

        _baseUrl = (providerOptions.BaseUrl ?? MengantarOptions.DefaultBaseUrl).TrimEnd('/');
        _apiKey =
            providerOptions.ApiKey
            ?? throw new InvalidOperationException(
                "Mengantar API key is not configured. Set 'Integrations:Providers:Mengantar:ApiKey' "
                    + "or the 'Integrations__Providers__Mengantar__ApiKey' environment variable."
            );
    }

    public async Task<MengantarSearchResponse> SearchAddressAsync(
        string keyword,
        string? etag = null,
        CancellationToken cancellationToken = default
    )
    {
        var uri = BuildUri(
            "address/search",
            new Dictionary<string, string?> { ["keyword"] = keyword }
        );

        using var request = new HttpRequestMessage(HttpMethod.Get, uri);
        if (!string.IsNullOrWhiteSpace(etag))
        {
            request.Headers.TryAddWithoutValidation("If-None-Match", etag);
        }

        using var response = await _httpClient.SendAsync(
            request,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken
        );

        if (response.StatusCode == HttpStatusCode.NotModified)
        {
            _logger.LogInformation(
                "Mengantar address search for '{Keyword}' is unchanged (304).",
                keyword
            );
            return new MengantarSearchResponse { NotModified = true };
        }

        await EnsureSuccessAsync(response, cancellationToken);

        var envelope = await response.Content.ReadFromJsonAsync<MengantarSearchEnvelope>(
            MengantarJson.Options,
            cancellationToken
        );

        var items = (envelope?.Data ?? [])
            .Where(d => d is not null && !string.IsNullOrWhiteSpace(d.Id))
            .ToList();

        return new MengantarSearchResponse
        {
            NotModified = false,
            Etag = response.Headers.ETag?.Tag,
            Items = items,
        };
    }

    private Uri BuildUri(string endpoint, IReadOnlyDictionary<string, string?>? query = null)
    {
        var queryString = string.Empty;
        if (query is { Count: > 0 })
        {
            queryString =
                "?"
                + string.Join(
                    "&",
                    query
                        .Where(p => p.Value is not null)
                        .Select(p =>
                            $"{Uri.EscapeDataString(p.Key)}={Uri.EscapeDataString(p.Value!)}"
                        )
                );
        }

        return new Uri($"{_baseUrl}/api/public/{_apiKey}/{endpoint}{queryString}");
    }

    private static async Task EnsureSuccessAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken
    )
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var message = $"Mengantar API request failed with status {(int)response.StatusCode}.";
        try
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            if (!string.IsNullOrWhiteSpace(body))
            {
                message += $" {body}";
            }
        }
        catch
        {
            // keep the generic message
        }

        throw new InvalidOperationException(message);
    }
}
