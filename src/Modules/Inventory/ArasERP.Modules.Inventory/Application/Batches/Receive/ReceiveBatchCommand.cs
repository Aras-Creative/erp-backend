using ArasERP.BuildingBlocks.Application;

namespace ArasERP.Modules.Inventory.Application.Batches.Receive;

public sealed class ReceiveBatchCommand : ICommand
{
    public required Guid ItemId { get; init; }
    public required Guid WarehouseId { get; init; }
    public required decimal ReceivedQty { get; init; }
    public required decimal UnitCost { get; init; }
    public required DateTime ReceivedAt { get; init; }
    public required string ReceiptNumber { get; init; }
    public string? RecordedBy { get; init; }
}
