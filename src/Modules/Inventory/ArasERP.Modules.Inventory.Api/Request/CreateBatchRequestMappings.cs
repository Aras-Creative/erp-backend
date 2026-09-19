using ArasERP.Modules.Inventory.Application.Batches.Create;

namespace ArasERP.Modules.Inventory.Api.Request;

public static class CreateBatchRequestMappings
{
    public static CreateBatchCommand ToCommand(this CreateBatchRequest request, Guid recordedBy) =>
        new()
        {
            ItemId = request.ItemId,
            WarehouseId = request.WarehouseId,
            ReceivedQty = request.ReceivedQty,
            UnitCost = request.UnitCost,
            ReceivedAt = request.ReceivedAt,
            SourceType = request.SourceType,
            ExternalReferenceNo = request.ExternalReferenceNo,
            Note = request.Note,
            ReceivedBy = request.ReceivedBy,
            RecordedBy = recordedBy,
        };
}
