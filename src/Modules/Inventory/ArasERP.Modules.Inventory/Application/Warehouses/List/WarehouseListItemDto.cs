using ArasERP.Modules.Inventory.Application.Warehouses.GetById;

namespace ArasERP.Modules.Inventory.Application.Warehouses.List;

public sealed record WarehouseListItemDto
{
    public required Guid WarehouseId { get; init; }

    public required string Name { get; init; }

    public required PersonInChargeData PersonInCharge { get; init; }

    public required AddressData Address { get; init; }

    public string? FullAddressText { get; init; }

    public static WarehouseListItemDto FromDetail(WarehouseDetailDto detail) =>
        new()
        {
            WarehouseId = detail.WarehouseId,
            Name = detail.Name,
            PersonInCharge = new PersonInChargeData(
                detail.PersonInCharge.Name,
                detail.PersonInCharge.Phone
            ),
            Address = new AddressData(
                detail.Address.Street,
                detail.Address.City,
                detail.Address.State,
                detail.Address.PostalCode,
                detail.Address.Country
            ),
            FullAddressText = detail.FullAddressText,
        };

    public sealed record PersonInChargeData(string Name, string? Phone);

    public sealed record AddressData(
        string Street,
        string City,
        string State,
        string PostalCode,
        string? Country
    );
}
