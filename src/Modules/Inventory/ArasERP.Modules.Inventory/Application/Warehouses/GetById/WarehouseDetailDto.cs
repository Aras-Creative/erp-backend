namespace ArasERP.Modules.Inventory.Application.Warehouses.GetById;

public sealed record WarehouseDetailDto
{
    public required Guid WarehouseId { get; init; }

    public required string Name { get; init; }

    public required bool IsActive { get; init; }

    public required PersonInChargeData PersonInCharge { get; init; }

    public required AddressData Address { get; init; }

    public string? FullAddressText { get; init; }

    public sealed record PersonInChargeData
    {
        public required string Name { get; init; }

        public string? Phone { get; init; }
    }

    public sealed record AddressData
    {
        public required Guid AddressId { get; init; }

        public required string SubDistrictName { get; init; }

        public required string DistrictName { get; init; }

        public required string CityName { get; init; }

        public required string ProvinceName { get; init; }

        public required string ZipCode { get; init; }
    }
}
