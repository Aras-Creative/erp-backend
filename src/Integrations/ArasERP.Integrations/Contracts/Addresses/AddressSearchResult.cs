namespace ArasERP.Integrations.Contracts.Addresses;

public sealed record AddressSearchResult(bool NotModified, string? Etag, IReadOnlyList<AddressSearchResultDto> Items)
{
    public static AddressSearchResult Unchanged() => new(true, null, []);
}
