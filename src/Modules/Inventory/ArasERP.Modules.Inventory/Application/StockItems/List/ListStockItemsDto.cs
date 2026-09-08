namespace ArasERP.Modules.Inventory.Application.StockItems.List;

public sealed class ListStockItemsDto
{
    public Guid Id { get; init; }

    public string Sku { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;

    public string Unit { get; init; } = string.Empty;

    public bool IsActive { get; init; }
}
