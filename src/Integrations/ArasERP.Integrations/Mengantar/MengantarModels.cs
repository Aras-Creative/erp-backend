using System.Text.Json;
using System.Text.Json.Serialization;

namespace ArasERP.Integrations.Mengantar;

public static class MengantarJson
{
    public static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);
}

public sealed class MengantarSearchResponse
{
    public bool NotModified { get; init; }

    public string? Etag { get; init; }

    public IReadOnlyList<MengantarAddressItem> Items { get; init; } = [];
}

public sealed class MengantarSearchEnvelope
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("data")]
    public List<MengantarAddressItem>? Data { get; set; }
}

public sealed class MengantarAddressItem
{
    [JsonPropertyName("_id")]
    public string? Id { get; set; }

    [JsonPropertyName("ORIGIN_CODE")]
    public string? OriginCode { get; set; }

    [JsonPropertyName("DESTINATION_CODE")]
    public string? DestinationCode { get; set; }

    [JsonPropertyName("PROVINCE_NAME")]
    public string? ProvinceName { get; set; }

    [JsonPropertyName("CITY_NAME")]
    public string? CityName { get; set; }

    [JsonPropertyName("DISTRICT_NAME")]
    public string? DistrictName { get; set; }

    [JsonPropertyName("SUBDISTRICT_NAME")]
    public string? SubDistrictName { get; set; }

    [JsonPropertyName("ZIP_CODE")]
    public string? ZipCode { get; set; }

    [JsonPropertyName("COUNTRY_NAME")]
    public string? CountryName { get; set; }

    [JsonPropertyName("closedSi")]
    public bool? ClosedSi { get; set; }

    [JsonPropertyName("unsupportedSi")]
    public bool? UnsupportedSi { get; set; }

    [JsonPropertyName("change")]
    public int? Change { get; set; }

    [JsonPropertyName("createdAt")]
    public string? CreatedAt { get; set; }

    [JsonPropertyName("updatedAt")]
    public string? UpdatedAt { get; set; }

    [JsonPropertyName("__v")]
    public int? Version { get; set; }

    [JsonPropertyName("CITY_NAME_SI")]
    public string? CityNameSi { get; set; }

    [JsonPropertyName("DESTINATION_CODE_SI")]
    public string? DestinationCodeSi { get; set; }

    [JsonPropertyName("ORIGIN_CODE_SI")]
    public string? OriginCodeSi { get; set; }

    [JsonPropertyName("CODE_SAP")]
    public string? CodeSap { get; set; }
}
