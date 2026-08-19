using ArasERP.BuildingBlocks.Domain.Abstractions;

namespace ArasERP.Modules.Address.Domain;

public sealed record AddressId(Guid Value) : TypedIdValueBase(Value)
{
    public static AddressId New() => new(Guid.NewGuid());
}
