using ArasERP.BuildingBlocks.Domain.Abstractions;

namespace ArasERP.Modules.Inventory.Domain.StockMovements;

public sealed record StockMovementId(Guid Value) : TypedIdValueBase(Value)
{
    public static StockMovementId New() => new(Guid.NewGuid());
}
