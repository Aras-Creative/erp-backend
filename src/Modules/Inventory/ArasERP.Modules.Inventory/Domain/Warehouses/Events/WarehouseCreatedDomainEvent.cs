using ArasERP.BuildingBlocks.Domain.Events;
using ArasERP.Modules.Inventory.Domain.Warehouses;

namespace ArasERP.Modules.Inventory.Domain.Warehouses.Events;

public sealed record WarehouseCreatedDomainEvent(WarehouseId WarehouseId) : IDomainEvent
{
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}
