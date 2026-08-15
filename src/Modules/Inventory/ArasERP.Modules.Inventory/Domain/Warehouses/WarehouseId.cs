using ArasERP.BuildingBlocks.Domain.Abstractions;

namespace ArasERP.Modules.Inventory.Domain.Warehouses;

public sealed record WarehouseId(Guid Value) : TypedIdValueBase(Value)
{
    public static WarehouseId New() => new(Guid.NewGuid());
}
