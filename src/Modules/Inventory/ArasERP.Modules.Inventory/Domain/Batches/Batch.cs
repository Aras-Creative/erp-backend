using ArasERP.BuildingBlocks.Domain.Abstractions;
using ArasERP.Modules.Inventory.Domain.StockItems;
using ArasERP.Modules.Inventory.Domain.Warehouses;

namespace ArasERP.Modules.Inventory.Domain.Batches;

public sealed class Batch : AggregateRoot<BatchId>
{
    public StockItemId ItemId { get; private set; } = null!;
    public WarehouseId WarehouseId { get; private set; } = null!;
    public DateTime ReceivedAt { get; private set; }
    public decimal ReceivedQty { get; private set; }
    public decimal RemainingQty { get; private set; }
    public decimal UnitCost { get; private set; }
    public BatchStatus Status { get; private set; } = null!;
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private Batch(
        BatchId id,
        StockItemId itemId,
        WarehouseId warehouseId,
        DateTime receivedAt,
        decimal receivedQty,
        decimal unitCost
    )
        : base(id)
    {
        ItemId = itemId;
        WarehouseId = warehouseId;
        ReceivedAt = receivedAt;
        ReceivedQty = receivedQty;
        RemainingQty = receivedQty;
        UnitCost = unitCost;
        Status = BatchStatus.Active;
        CreatedAt = DateTime.UtcNow;
    }

    private Batch() { }

    public static Batch Create(
        StockItemId itemId,
        WarehouseId warehouseId,
        DateTime receivedAt,
        decimal receivedQty,
        decimal unitCost
    )
    {
        ArgumentNullException.ThrowIfNull(itemId);
        ArgumentNullException.ThrowIfNull(warehouseId);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(receivedQty, 0);
        ArgumentOutOfRangeException.ThrowIfNegative(unitCost);
        if (receivedAt > DateTime.UtcNow)
            throw new ArgumentOutOfRangeException(
                nameof(receivedAt),
                "Entry date cannot be in the future."
            );

        return new Batch(
            BatchId.New(),
            itemId,
            warehouseId,
            receivedAt,
            receivedQty,
            unitCost
        );
    }

    public void Consume(decimal qty, DateTime? updatedAt = null)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(qty);
        if (qty > RemainingQty)
            throw new InvalidOperationException("Cannot consume more than the remaining quantity.");

        RemainingQty -= qty;
        if (RemainingQty == 0)
            Status = BatchStatus.Exhausted;

        UpdatedAt = updatedAt ?? DateTime.UtcNow;
    }
}
