namespace ArasERP.Modules.Inventory.Contracts.Warehouses;

public sealed class WarehouseResponse
{
    public required Guid WarehouseId { get; init; }

    public required string Name { get; init; }

    public required PersonInChargeData PersonInCharge { get; init; }

    public required AddressData Address { get; init; }

    public string? FullAddressText { get; init; }

    public sealed record PersonInChargeData(string Name, string? Phone);

    public sealed record AddressData(
        string Street,
        string City,
        string State,
        string PostalCode,
        string? Country
    );
}
