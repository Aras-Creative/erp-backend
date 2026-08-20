using ArasERP.BuildingBlocks.Application;

namespace ArasERP.Modules.Inventory.Application.Warehouses.Update;

public sealed class UpdateWarehouseCommand : ICommand
{
    public string WarehouseId { get; set; } = null!;
    public required string Name { get; init; }

    public required Guid AddressId { get; init; }

    public required PersonInChargeData PersonInCharge { get; init; }

    public string? FullAddressText { get; init; }

    public sealed record PersonInChargeData
    {
        public required string Name { get; init; }

        public string? Phone { get; init; }
    }
}
