namespace ArasERP.Modules.Inventory.Application.StockMovements.List;

public sealed class ListStockMovementsDto
{
    public Guid Id { get; init; }

    public Guid ItemId { get; init; }

    public Guid WarehouseId { get; init; }

    public Guid? BatchId { get; init; }

    public string Direction { get; init; } = string.Empty;

    public decimal Quantity { get; init; }

    public string SourceType { get; init; } = string.Empty;

    public Guid? SourceReferenceId { get; init; }

    public string? ExternalReferenceNo { get; init; }

    public string? Note { get; init; }

    public DateTime CreatedAt { get; init; }

    public Guid RecordedBy { get; init; }

    public string? ReceivedBy { get; init; }
}