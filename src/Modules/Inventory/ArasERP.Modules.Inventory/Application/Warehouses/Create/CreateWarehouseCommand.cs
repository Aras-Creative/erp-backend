using ArasERP.BuildingBlocks.Application;

namespace ArasERP.Modules.Inventory.Application.Warehouses.Create;

public sealed class CreateWarehouseCommand : ICommand
{
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
