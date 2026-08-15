namespace ArasERP.BuildingBlocks.Domain.Abstractions;

public abstract class AggregateRoot<TId> : Entity<TId>, IAggregateRoot<TId>
    where TId : notnull
{
    protected AggregateRoot(TId id)
        : base(id) { }

    protected AggregateRoot() { }
}
