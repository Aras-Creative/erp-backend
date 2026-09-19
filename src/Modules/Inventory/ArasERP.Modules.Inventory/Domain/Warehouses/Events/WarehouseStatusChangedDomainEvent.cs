using ArasERP.BuildingBlocks.Domain.Events;

namespace ArasERP.Modules.Inventory.Domain.Warehouses.Events;

public sealed record WarehouseStatusChangedDomainEvent(WarehouseId WarehouseId, bool IsActive)
    : IDomainEvent
{
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}
