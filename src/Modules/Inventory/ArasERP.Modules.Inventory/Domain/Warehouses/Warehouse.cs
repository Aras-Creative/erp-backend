using ArasERP.BuildingBlocks.Domain.Abstractions;
using ArasERP.Modules.Inventory.Domain.Warehouses.Events;
using ArasERP.Modules.Inventory.Domain.Warehouses.ValueObjects;

namespace ArasERP.Modules.Inventory.Domain.Warehouses;

public sealed class Warehouse : AggregateRoot<WarehouseId>
{
    private Warehouse(
        WarehouseId id,
        string name,
        WarehousePersonInCharge personInCharge,
        WarehouseAddress address,
        string? fullAddressText
    )
        : base(id)
    {
        Name = name;
        PersonInCharge = personInCharge;
        Address = address;
        FullAddressText = fullAddressText;
        IsActive = true;
    }

    private Warehouse() { }

    public static Warehouse Create(
        string name,
        WarehousePersonInCharge personInCharge,
        WarehouseAddress address,
        string? fullAddressText = null
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(personInCharge);
        ArgumentNullException.ThrowIfNull(address);

        var warehouse = new Warehouse(
            WarehouseId.New(),
            name,
            personInCharge,
            address,
            fullAddressText
        );
        warehouse.AddDomainEvent(new WarehouseCreatedDomainEvent(warehouse.Id));
        return warehouse;
    }

    public void Update(
        string name,
        WarehousePersonInCharge personInCharge,
        WarehouseAddress address,
        string? fullAddressText = null
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(personInCharge);
        ArgumentNullException.ThrowIfNull(address);

        Name = name;
        PersonInCharge = personInCharge;
        Address = address;
        FullAddressText = fullAddressText;

        AddDomainEvent(new WarehouseUpdatedDomainEvent(Id));
    }

    public void ToggleStatus()
    {
        IsActive = !IsActive;
        AddDomainEvent(new WarehouseStatusChangedDomainEvent(Id, IsActive));
    }

    public void Delete()
    {
        // TODO: reject delete when the warehouse still has stock (pending Stock aggregate).
        if (IsDeleted)
        {
            return;
        }

        IsDeleted = true;
        DeletedAtUtc = DateTime.UtcNow;
        AddDomainEvent(new WarehouseDeletedDomainEvent(Id));
    }

    public bool IsActive { get; private set; }

    public bool IsDeleted { get; private set; }

    public DateTime? DeletedAtUtc { get; private set; }

    public string Name { get; private set; } = null!;

    public WarehousePersonInCharge PersonInCharge { get; private set; } = null!;

    public WarehouseAddress Address { get; private set; } = null!;

    public string? FullAddressText { get; private set; }
}
