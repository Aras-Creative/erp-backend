using System.Net;
using System.Net.Http.Json;
using ArasERP.BuildingBlocks.Application;
using ArasERP.Integrations.Abstractions;
using ArasERP.Integrations.Contracts.Addresses;
using ArasERP.Integrations.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ArasERP.Integrations.Infrastructure.Providers.Mengantar;

public sealed class MengantarProvider : IShippingProvider
{
    private readonly HttpClient _httpClient;

    private readonly ILogger<MengantarProvider> _logger;

    private readonly string _baseUrl;

    private readonly ProviderOptions _providerOptions;

    public MengantarProvider(
        HttpClient httpClient,
        IOptions<IntegrationsOptions> options,
        ILogger<MengantarProvider> logger
    )
    {
        _httpClient = httpClient;
        _logger = logger;
        _providerOptions =
            options.Value.Providers?.GetValueOrDefault(MengantarOptions.ProviderName)
            ?? new ProviderOptions();
        _baseUrl = (_providerOptions.BaseUrl ?? MengantarOptions.DefaultBaseUrl).TrimEnd('/');
    }

    public string Name => MengantarOptions.ProviderName;

    public async Task<AddressSearchResult> SearchAddressesAsync(
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
            _logger.LogInformation("Mengantar address search for '{Keyword}' is unchanged (304).", keyword);
            return AddressSearchResult.Unchanged();
        }

        await EnsureSuccessAsync(response, cancellationToken);

        var envelope = await response.Content.ReadFromJsonAsync<MengantarSearchEnvelope>(
            MengantarJson.Options,
            cancellationToken
        );

        var items = (envelope?.Data ?? [])
            .Where(d => d is not null && !string.IsNullOrWhiteSpace(d.Id))
            .Select(
                d =>
                    new AddressSearchResultDto(
                        d.Id!,
                        d.OriginCode ?? string.Empty,
                        d.DestinationCode ?? string.Empty,
                        d.ProvinceName ?? string.Empty,
                        d.CityName ?? string.Empty,
                        d.DistrictName ?? string.Empty,
                        d.SubDistrictName ?? string.Empty,
                        d.ZipCode ?? string.Empty
                    )
            )
            .ToList();

        return new AddressSearchResult(false, response.Headers.ETag?.Tag, items);
    }

    private Uri BuildUri(string endpoint, IReadOnlyDictionary<string, string?>? query = null)
    {
        var apiKey = RequireApiKey();
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

        return new Uri($"{_baseUrl}/api/public/{apiKey}/{endpoint}{queryString}");
    }

    private string RequireApiKey()
    {
        if (string.IsNullOrWhiteSpace(_providerOptions.ApiKey))
        {
            throw new ValidationException(
                "Mengantar API key is not configured. Set 'Integrations:Providers:Mengantar:ApiKey' "
                    + "or the 'Integrations__Providers__Mengantar__ApiKey' environment variable."
            );
        }

        return _providerOptions.ApiKey;
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

        throw new ValidationException(message);
    }
}
