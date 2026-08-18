namespace ArasERP.Modules.Address.Contracts.Addresses;

public sealed record AddressSyncItem(
    string ExternalId,
    string OriginCode,
    string DestinationCode,
    string ProvinceName,
    string CityName,
    string DistrictName,
    string SubDistrictName,
    string ZipCode
);
