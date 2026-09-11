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
    public string? Note { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public string? CreatedBy { get; private set; }

    private StockMovement(
        StockMovementId id,
        StockItemId itemId,
        WarehouseId warehouseId,
        Direction direction,
        decimal quantity,
        SourceType sourceType,
        Guid? sourceReferenceId,
        BatchId? batchId,
        string? note,
        string? createdBy
    )
        : base(id)
    {
        ItemId = itemId;
        WarehouseId = warehouseId;
        Direction = direction;
        Quantity = quantity;
        SourceType = sourceType;
        SourceReferenceId = sourceReferenceId;
        BatchId = batchId;
        Note = note;
        CreatedAt = DateTime.UtcNow;
        CreatedBy = createdBy;
    }

    private StockMovement() { }

    public static StockMovement Create(
        StockItemId itemId,
        WarehouseId warehouseId,
        Direction direction,
        decimal quantity,
        SourceType sourceType,
        Guid? sourceReferenceId = null,
        BatchId? batchId = null,
        string? note = null,
        string? createdBy = null
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
            batchId,
            note?.Trim(),
            createdBy
        );
    }
}