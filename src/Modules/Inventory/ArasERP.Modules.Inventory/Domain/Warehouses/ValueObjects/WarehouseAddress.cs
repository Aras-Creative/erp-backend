using ArasERP.BuildingBlocks.Domain.Abstractions;

namespace ArasERP.Modules.Inventory.Domain.Warehouses.ValueObjects;

public sealed class WarehouseAddress : ValueObject
{
    private WarehouseAddress(
        string street,
        string city,
        string state,
        string postalCode,
        string? country
    )
    {
        Street = street;
        City = city;
        State = state;
        PostalCode = postalCode;
        Country = country;
    }

    public static WarehouseAddress Create(
        string street,
        string city,
        string state,
        string postalCode,
        string? country = null
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(street);
        ArgumentException.ThrowIfNullOrWhiteSpace(city);
        ArgumentException.ThrowIfNullOrWhiteSpace(state);
        ArgumentException.ThrowIfNullOrWhiteSpace(postalCode);

        return new WarehouseAddress(street, city, state, postalCode, country);
    }

    public string Street { get; }

    public string City { get; }

    public string State { get; }

    public string PostalCode { get; }

    public string? Country { get; }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Street;
        yield return City;
        yield return State;
        yield return PostalCode;
        yield return Country;
    }
}
