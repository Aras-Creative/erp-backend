using ArasERP.BuildingBlocks.Domain.Abstractions;
using ArasERP.Modules.Inventory.Domain.StockItems;
using ArasERP.Modules.Inventory.Domain.Warehouses;

namespace ArasERP.Modules.Inventory.Domain.StockLevels;

public sealed class StockLevel : AggregateRoot<StockLevelId>
{
    public StockItemId ItemId { get; private set; } = null!;
    public WarehouseId WarehouseId { get; private set; } = null!;
    public decimal OnHandQty { get; private set; }
    public decimal ReservedQty { get; private set; }
    public uint RowVersion { get; set; }
    public DateTime UpdatedAt { get; private set; }
    public string? UpdatedBy { get; private set; }

    public decimal AvailableQty => OnHandQty - ReservedQty;

    public bool HasStock => OnHandQty > 0 || ReservedQty > 0;

    private StockLevel(StockItemId itemId, WarehouseId warehouseId)
        : base(StockLevelId.New())
    {
        ItemId = itemId;
        WarehouseId = warehouseId;
        OnHandQty = 0;
        ReservedQty = 0;
        UpdatedAt = DateTime.UtcNow;
    }

    private StockLevel() { }

    public static StockLevel Create(StockItemId itemId, WarehouseId warehouseId)
    {
        ArgumentNullException.ThrowIfNull(itemId);
        ArgumentNullException.ThrowIfNull(warehouseId);

        return new StockLevel(itemId, warehouseId);
    }

    public void Receive(decimal qty, string? updatedBy = null)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(qty, 0);
        OnHandQty += qty;
        Touch(updatedBy);
    }

    public void Reserve(decimal qty, string? updatedBy = null)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(qty, 0);
        if (qty > AvailableQty)
            throw new InvalidOperationException("Cannot reserve more than the available quantity.");

        ReservedQty += qty;
        Touch(updatedBy);
    }

    public void Release(decimal qty, string? updatedBy = null)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(qty, 0);
        if (qty > ReservedQty)
            throw new InvalidOperationException("Cannot release more than the reserved quantity.");

        ReservedQty -= qty;
        Touch(updatedBy);
    }

    public void Issue(decimal qty, string? updatedBy = null)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(qty, 0);
        if (qty > OnHandQty)
            throw new InvalidOperationException("Cannot issue more than the on-hand quantity.");

        OnHandQty -= qty;
        ReservedQty -= Math.Min(ReservedQty, qty);
        Touch(updatedBy);
    }

    private void Touch(string? updatedBy = null)
    {
        UpdatedAt = DateTime.UtcNow;
        if (!string.IsNullOrWhiteSpace(updatedBy))
            UpdatedBy = updatedBy;
    }
}
