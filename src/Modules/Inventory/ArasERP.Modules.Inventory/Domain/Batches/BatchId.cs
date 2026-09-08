using ArasERP.BuildingBlocks.Domain.Abstractions;

namespace ArasERP.Modules.Inventory.Domain.Batches;

public sealed record BatchId(Guid Value) : TypedIdValueBase(Value)
{
    public static BatchId New() => new(Guid.NewGuid());
}
