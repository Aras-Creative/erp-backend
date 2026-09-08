using ArasERP.BuildingBlocks.Domain.Abstractions;

namespace ArasERP.Modules.Inventory.Domain.StockLevels;

public sealed record StockLevelId(Guid Value) : TypedIdValueBase(Value)
{
    public static StockLevelId New() => new(Guid.NewGuid());
}
