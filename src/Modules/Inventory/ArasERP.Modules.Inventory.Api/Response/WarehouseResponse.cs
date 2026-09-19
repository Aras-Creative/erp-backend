namespace ArasERP.Modules.Inventory.Api.Response;

public sealed class WarehouseResponse
{
    public required Guid WarehouseId { get; init; }
    public required string Name { get; init; }
    public required bool IsActive { get; init; }
    public required PersonInChargeData PersonInCharge { get; init; }
    public required AddressData Address { get; init; }
    public string? FullAddressText { get; init; }

    public sealed record PersonInChargeData(string Name, string? Phone);

    public sealed record AddressData(
        Guid AddressId,
        string SubDistrictName,
        string DistrictName,
        string CityName,
        string ProvinceName,
        string ZipCode
    );
}
