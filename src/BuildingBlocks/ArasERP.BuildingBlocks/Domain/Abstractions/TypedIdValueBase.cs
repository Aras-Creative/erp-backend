namespace ArasERP.BuildingBlocks.Domain.Abstractions;

public abstract record TypedIdValueBase(Guid Value) : IComparable<TypedIdValueBase>
{
    public int CompareTo(TypedIdValueBase? other) =>
        other is null ? 1 : Value.CompareTo(other.Value);

    public override string ToString() => Value.ToString();
}
