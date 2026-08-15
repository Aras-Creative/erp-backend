using ArasERP.BuildingBlocks.Domain.Events;

namespace ArasERP.Modules.Inventory.Domain.Warehouses.Events;

public sealed record WarehouseDeletedDomainEvent(WarehouseId WarehouseId) : IDomainEvent
{
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}
