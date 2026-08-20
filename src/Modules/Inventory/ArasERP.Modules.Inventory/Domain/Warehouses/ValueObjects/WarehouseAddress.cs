using ArasERP.BuildingBlocks.Domain.Abstractions;

namespace ArasERP.Modules.Inventory.Domain.Warehouses.ValueObjects;

public sealed class WarehouseAddress : ValueObject
{
    private WarehouseAddress(
        string subDistrictName,
        string districtName,
        string cityName,
        string provinceName,
        string zipCode
    )
    {
        SubDistrictName = subDistrictName;
        DistrictName = districtName;
        CityName = cityName;
        ProvinceName = provinceName;
        ZipCode = zipCode;
    }

    public static WarehouseAddress Create(
        string subDistrictName,
        string districtName,
        string cityName,
        string provinceName,
        string zipCode
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(subDistrictName);
        ArgumentException.ThrowIfNullOrWhiteSpace(districtName);
        ArgumentException.ThrowIfNullOrWhiteSpace(cityName);
        ArgumentException.ThrowIfNullOrWhiteSpace(provinceName);
        ArgumentException.ThrowIfNullOrWhiteSpace(zipCode);

        return new WarehouseAddress(subDistrictName, districtName, cityName, provinceName, zipCode);
    }

    public string SubDistrictName { get; }

    public string DistrictName { get; }

    public string CityName { get; }

    public string ProvinceName { get; }

    public string ZipCode { get; }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return SubDistrictName;
        yield return DistrictName;
        yield return CityName;
        yield return ProvinceName;
        yield return ZipCode;
    }
}
