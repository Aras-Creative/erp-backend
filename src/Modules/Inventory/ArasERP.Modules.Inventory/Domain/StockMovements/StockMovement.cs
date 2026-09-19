using ArasERP.BuildingBlocks.Domain.Abstractions;
using ArasERP.Modules.Inventory.Domain.Batches;
using ArasERP.Modules.Inventory.Domain.StockItems;
using ArasERP.Modules.Inventory.Domain.Warehouses;

namespace ArasERP.Modules.Inventory.Domain.StockMovements;

public sealed class StockMovement : AggregateRoot<StockMovementId>
{
    public StockItemId ItemId { get; private set; } = null!;
    public WarehouseId WarehouseId { get; private set; } = null!;
    public BatchId? BatchId { get; private set; }
    public Direction Direction { get; private set; } = null!;
    public decimal Quantity { get; private set; }
    public decimal UnitCost { get; private set; }
    public string Currency { get; private set; } = null!;
    public SourceType SourceType { get; private set; } = null!;
    public Guid? SourceReferenceId { get; private set; }
    public string? ExternalReferenceNo { get; private set; }
    public string? Note { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public Guid RecordedBy { get; private set; }
    public string? ReceivedBy { get; private set; }

    public decimal Total => Quantity * UnitCost;

    private StockMovement(
        StockMovementId id,
        StockItemId itemId,
        WarehouseId warehouseId,
        Direction direction,
        decimal quantity,
        decimal unitCost,
        string currency,
        SourceType sourceType,
        Guid? sourceReferenceId,
        string? externalReferenceNo,
        BatchId? batchId,
        string? note,
        Guid recordedBy,
        string? receivedBy
    )
        : base(id)
    {
        ItemId = itemId;
        WarehouseId = warehouseId;
        Direction = direction;
        Quantity = quantity;
        UnitCost = unitCost;
        Currency = currency;
        SourceType = sourceType;
        SourceReferenceId = sourceReferenceId;
        ExternalReferenceNo = externalReferenceNo;
        BatchId = batchId;
        Note = note;
        CreatedAt = DateTime.UtcNow;
        RecordedBy = recordedBy;
        ReceivedBy = receivedBy;
    }

    private StockMovement() { }

    /// <summary>
    /// Creates a stock movement and snapshots the unit cost and currency at the time of the event
    /// so historical value never changes with later price corrections.
    /// For movements that reference a batch, <paramref name="unitCost"/> must equal the batch's
    /// <see cref="Batches.Batch.UnitCost"/>; callers are responsible for passing the batch cost.
    /// </summary>
    public static StockMovement Create(
        StockItemId itemId,
        WarehouseId warehouseId,
        Direction direction,
        decimal quantity,
        SourceType sourceType,
        decimal unitCost,
        string currency = "IDR",
        Guid? sourceReferenceId = null,
        string? externalReferenceNo = null,
        BatchId? batchId = null,
        string? note = null,
        Guid recordedBy = default,
        string? receivedBy = null
    )
    {
        ArgumentNullException.ThrowIfNull(itemId);
        ArgumentNullException.ThrowIfNull(warehouseId);
        ArgumentNullException.ThrowIfNull(direction);
        ArgumentNullException.ThrowIfNull(sourceType);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(quantity, 0);
        ArgumentOutOfRangeException.ThrowIfNegative(unitCost);

        return new StockMovement(
            StockMovementId.New(),
            itemId,
            warehouseId,
            direction,
            quantity,
            unitCost,
            (currency ?? "IDR").Trim().ToUpperInvariant(),
            sourceType,
            sourceReferenceId,
            externalReferenceNo?.Trim(),
            batchId,
            note?.Trim(),
            recordedBy,
            receivedBy?.Trim()
        );
    }
}
