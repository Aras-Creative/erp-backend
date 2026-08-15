using ArasERP.BuildingBlocks.Domain.Abstractions;

namespace ArasERP.Modules.Inventory.Domain.Warehouses.ValueObjects;

public sealed class WarehousePersonInCharge : ValueObject
{
    private WarehousePersonInCharge(string name, string? phone)
    {
        Name = name;
        Phone = phone;
    }

    public static WarehousePersonInCharge Create(string name, string? phone = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return new WarehousePersonInCharge(name, phone);
    }

    public string Name { get; }

    public string? Phone { get; }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Name;
        yield return Phone;
    }
}
