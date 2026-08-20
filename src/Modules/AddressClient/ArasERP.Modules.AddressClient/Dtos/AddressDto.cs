namespace ArasERP.Modules.AddressClient.Dtos;

public sealed class AddressDto
{
    public Guid AddressId { get; init; }

    public string DestinationCode { get; init; } = null!;

    public string OriginCode { get; init; } = null!;

    public string ProvinceName { get; init; } = null!;

    public string CityName { get; init; } = null!;

    public string DistrictName { get; init; } = null!;

    public string SubDistrictName { get; init; } = null!;

    public string ZipCode { get; init; } = null!;
}
