using ArasERP.BuildingBlocks.Domain.Abstractions;

namespace ArasERP.Modules.Address.Domain;

public sealed class Address : AggregateRoot<AddressId>
{
    private Address(
        AddressId id,
        string destinationCode,
        string originCode,
        string provinceName,
        string cityName,
        string districtName,
        string subDistrictName,
        string zipCode,
        string? externalId
    )
        : base(id)
    {
        DestinationCode = destinationCode;
        OriginCode = originCode;
        ProvinceName = provinceName;
        CityName = cityName;
        DistrictName = districtName;
        SubDistrictName = subDistrictName;
        ZipCode = zipCode;
        ExternalId = externalId;
        CreatedAtUtc = DateTime.UtcNow;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    private Address() { }

    public static Address Create(
        string destinationCode,
        string originCode,
        string provinceName,
        string cityName,
        string districtName,
        string subDistrictName,
        string zipCode,
        string? externalId = null
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(destinationCode);
        ArgumentException.ThrowIfNullOrWhiteSpace(originCode);
        ArgumentException.ThrowIfNullOrWhiteSpace(provinceName);
        ArgumentException.ThrowIfNullOrWhiteSpace(cityName);
        ArgumentException.ThrowIfNullOrWhiteSpace(districtName);
        ArgumentException.ThrowIfNullOrWhiteSpace(subDistrictName);
        ArgumentException.ThrowIfNullOrWhiteSpace(zipCode);

        return new Address(
            AddressId.New(),
            destinationCode,
            originCode,
            provinceName,
            cityName,
            districtName,
            subDistrictName,
            zipCode,
            externalId
        );
    }

    public void Update(
        string destinationCode,
        string originCode,
        string provinceName,
        string cityName,
        string districtName,
        string subDistrictName,
        string zipCode,
        string? externalId = null
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(destinationCode);
        ArgumentException.ThrowIfNullOrWhiteSpace(originCode);
        ArgumentException.ThrowIfNullOrWhiteSpace(provinceName);
        ArgumentException.ThrowIfNullOrWhiteSpace(cityName);
        ArgumentException.ThrowIfNullOrWhiteSpace(districtName);
        ArgumentException.ThrowIfNullOrWhiteSpace(subDistrictName);
        ArgumentException.ThrowIfNullOrWhiteSpace(zipCode);

        DestinationCode = destinationCode;
        OriginCode = originCode;
        ProvinceName = provinceName;
        CityName = cityName;
        DistrictName = districtName;
        SubDistrictName = subDistrictName;
        ZipCode = zipCode;
        ExternalId = externalId;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public string DestinationCode { get; private set; } = null!;

    public string OriginCode { get; private set; } = null!;

    public string ProvinceName { get; private set; } = null!;

    public string CityName { get; private set; } = null!;

    public string DistrictName { get; private set; } = null!;

    public string SubDistrictName { get; private set; } = null!;

    public string ZipCode { get; private set; } = null!;

    public string? ExternalId { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime UpdatedAtUtc { get; private set; }
}
