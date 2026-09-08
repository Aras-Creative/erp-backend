namespace ArasERP.Modules.Inventory.Api.StockItems;

public sealed class StockItemResponse
{
    public Guid Id { get; init; }

    public string Sku { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;

    public string Unit { get; init; } = string.Empty;

    public bool IsActive { get; init; }

    public Guid? WarehouseId { get; init; }

    public string? WarehouseName { get; init; }

    public decimal OnHandQty { get; init; }

    public decimal ReservedQty { get; init; }

    public decimal AvailableQty { get; init; }
}
