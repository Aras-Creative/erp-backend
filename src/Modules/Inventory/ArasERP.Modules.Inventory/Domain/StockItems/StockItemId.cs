using ArasERP.BuildingBlocks.Domain.Abstractions;

namespace ArasERP.Modules.Inventory.Domain.StockItems;

public sealed record StockItemId(Guid Value): TypedIdValueBase(Value)
{
    public static StockItemId New() => new StockItemId(Guid.NewGuid());
}
