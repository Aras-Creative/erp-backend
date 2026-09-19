using ArasERP.BuildingBlocks.Domain.Abstractions;

namespace ArasERP.Modules.Inventory.Domain.Warehouses.ValueObjects;

public sealed class WarehouseAddress : ValueObject
{
    private WarehouseAddress(
        Guid addressId,
        string subDistrictName,
        string districtName,
        string cityName,
        string provinceName,
        string zipCode
    )
    {
        AddressId = addressId;
        SubDistrictName = subDistrictName;
        DistrictName = districtName;
        CityName = cityName;
        ProvinceName = provinceName;
        ZipCode = zipCode;
    }

    public static WarehouseAddress Create(
        Guid addressId,
        string subDistrictName,
        string districtName,
        string cityName,
        string provinceName,
        string zipCode
    )
    {
        if (addressId == Guid.Empty)
        {
            throw new ArgumentException(
                $"'{nameof(addressId)}' cannot be empty.",
                nameof(addressId)
            );
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(subDistrictName);
        ArgumentException.ThrowIfNullOrWhiteSpace(districtName);
        ArgumentException.ThrowIfNullOrWhiteSpace(cityName);
        ArgumentException.ThrowIfNullOrWhiteSpace(provinceName);
        ArgumentException.ThrowIfNullOrWhiteSpace(zipCode);

        return new WarehouseAddress(
            addressId,
            subDistrictName,
            districtName,
            cityName,
            provinceName,
            zipCode
        );
    }

    public Guid AddressId { get; }

    public string SubDistrictName { get; }

    public string DistrictName { get; }

    public string CityName { get; }

    public string ProvinceName { get; }

    public string ZipCode { get; }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return AddressId;
        yield return SubDistrictName;
        yield return DistrictName;
        yield return CityName;
        yield return ProvinceName;
        yield return ZipCode;
    }
}
