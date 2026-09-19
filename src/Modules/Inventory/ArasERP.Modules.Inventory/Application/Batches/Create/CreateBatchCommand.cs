using ArasERP.BuildingBlocks.Application;

namespace ArasERP.Modules.Inventory.Application.Batches.Create;

public sealed class CreateBatchCommand : ICommand
{
    public required Guid ItemId { get; init; }
    public required Guid WarehouseId { get; init; }
    public required decimal ReceivedQty { get; init; }
    public required decimal UnitCost { get; init; }
    public required DateTime ReceivedAt { get; init; }
    public required SourceTypeEnum SourceType { get; init; }
    public string? ExternalReferenceNo { get; init; }
    public string? Note { get; init; }
    public string? ReceivedBy { get; init; }
    public required Guid RecordedBy { get; init; }
}
