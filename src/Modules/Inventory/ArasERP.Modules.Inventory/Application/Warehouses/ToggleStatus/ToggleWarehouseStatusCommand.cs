using ArasERP.BuildingBlocks.Application;

namespace ArasERP.Modules.Inventory.Application.Warehouses.ToggleStatus;

public sealed class ToggleWarehouseStatusCommand : ICommand
{
    public required string WarehouseId { get; set; }
}
