using ArasERP.BuildingBlocks.Application;
using ArasERP.Modules.Inventory.Application.StockMovements.List;

namespace ArasERP.Modules.Inventory.Api.Response;

public static class StockMovementResponseMappings
{
    public static PagedList<StockMovementResponse> ToResponse(
        this PagedList<ListStockMovementsDto> items
    ) => items.Map(i => i.ToResponse());

    public static StockMovementResponse ToResponse(this ListStockMovementsDto item) =>
        new()
        {
            Id = item.Id,
            ItemId = item.ItemId,
            ItemName = item.ItemName,
            WarehouseId = item.WarehouseId,
            WarehouseName = item.WarehouseName,
            BatchId = item.BatchId,
            Direction = item.Direction,
            Quantity = item.Quantity,
            SourceType = item.SourceType,
            SourceReferenceId = item.SourceReferenceId,
            ExternalReferenceNo = item.ExternalReferenceNo,
            Note = item.Note,
            CreatedAt = item.CreatedAt,
            RecordedBy = item.RecordedBy,
            ReceivedBy = item.ReceivedBy,
        };
}
