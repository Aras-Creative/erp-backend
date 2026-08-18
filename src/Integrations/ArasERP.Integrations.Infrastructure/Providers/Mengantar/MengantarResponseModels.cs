using System.Text.Json;
using System.Text.Json.Serialization;

namespace ArasERP.Integrations.Infrastructure.Providers.Mengantar;

internal static class MengantarJson
{
    public static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);
}

internal sealed class MengantarSearchEnvelope
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("data")]
    public List<MengantarAddressItem>? Data { get; set; }
}

internal sealed class MengantarAddressItem
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
}
