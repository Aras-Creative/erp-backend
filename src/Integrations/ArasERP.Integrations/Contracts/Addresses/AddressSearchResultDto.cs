namespace ArasERP.Integrations.Contracts.Addresses;

public sealed record AddressSearchResultDto(
    string ProviderId,
    string OriginCode,
    string DestinationCode,
    string ProvinceName,
    string CityName,
    string DistrictName,
    string SubDistrictName,
    string ZipCode
);
