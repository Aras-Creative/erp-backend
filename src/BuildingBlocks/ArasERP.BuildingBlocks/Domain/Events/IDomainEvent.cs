namespace ArasERP.BuildingBlocks.Domain.Events;

public interface IDomainEvent
{
    DateTime OccurredAt { get; }
}
