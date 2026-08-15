using ArasERP.BuildingBlocks.Application;

namespace ArasERP.Modules.Inventory.Application.Warehouses.Delete;

public sealed class DeleteWarehouseCommand : ICommand
{
    public required string WarehouseId { get; set; }
}
