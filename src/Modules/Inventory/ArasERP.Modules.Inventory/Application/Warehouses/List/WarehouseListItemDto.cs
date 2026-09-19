using ArasERP.Modules.Inventory.Application.Warehouses.GetById;

namespace ArasERP.Modules.Inventory.Application.Warehouses.List;

public sealed record WarehouseListItemDto
{
    public required Guid WarehouseId { get; init; }

    public required string Name { get; init; }

    public required bool IsActive { get; init; }

    public required PersonInChargeData PersonInCharge { get; init; }

    public required AddressData Address { get; init; }

    public string? FullAddressText { get; init; }

    public static WarehouseListItemDto FromDetail(WarehouseDetailDto detail) =>
        new()
        {
            WarehouseId = detail.WarehouseId,
            Name = detail.Name,
            IsActive = detail.IsActive,
            PersonInCharge = new PersonInChargeData(
                detail.PersonInCharge.Name,
                detail.PersonInCharge.Phone
            ),
            Address = new AddressData(
                detail.Address.AddressId,
                detail.Address.SubDistrictName,
                detail.Address.DistrictName,
                detail.Address.CityName,
                detail.Address.ProvinceName,
                detail.Address.ZipCode
            ),
            FullAddressText = detail.FullAddressText,
        };

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
