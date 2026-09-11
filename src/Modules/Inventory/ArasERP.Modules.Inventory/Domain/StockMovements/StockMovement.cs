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
    public SourceType SourceType { get; private set; } = null!;
    public Guid? SourceReferenceId { get; private set; }
    public string? ExternalReferenceNo { get; private set; }
    public string? Note { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public Guid RecordedBy { get; private set; }
    public string? ReceivedBy { get; private set; }

    private StockMovement(
        StockMovementId id,
        StockItemId itemId,
        WarehouseId warehouseId,
        Direction direction,
        decimal quantity,
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

    public static StockMovement Create(
        StockItemId itemId,
        WarehouseId warehouseId,
        Direction direction,
        decimal quantity,
        SourceType sourceType,
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

        return new StockMovement(
            StockMovementId.New(),
            itemId,
            warehouseId,
            direction,
            quantity,
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