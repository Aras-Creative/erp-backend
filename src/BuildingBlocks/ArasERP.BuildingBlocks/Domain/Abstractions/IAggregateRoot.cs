using ArasERP.BuildingBlocks.Domain.Events;

namespace ArasERP.BuildingBlocks.Domain.Abstractions;

public interface IAggregateRoot<TId> : IEntity<TId>
    where TId : notnull
{
    IReadOnlyCollection<IDomainEvent> DomainEvents { get; }

    void ClearDomainEvents();
}
