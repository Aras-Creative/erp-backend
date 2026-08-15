namespace ArasERP.BuildingBlocks.Domain.Abstractions;

public interface IEntity<TId>
    where TId : notnull
{
    TId Id { get; }
}
