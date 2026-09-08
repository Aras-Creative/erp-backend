using ArasERP.BuildingBlocks.Application;
using ArasERP.Modules.Inventory.Application.StockItems.List;

namespace ArasERP.Modules.Inventory.Api.StockItems;

public static class StockItemResponseMappings
{
    public static PagedList<StockItemResponse> ToResponse(this PagedList<ListStockItemsDto> items)
    {
        return items.Map(i => i.ToResponse());
    }

    public static StockItemResponse ToResponse(this ListStockItemsDto item) =>
        new()
        {
            Id = item.Id,
            Sku = item.Sku,
            Name = item.Name,
            Unit = item.Unit,
            IsActive = item.IsActive,
            WarehouseId = item.WarehouseId,
            WarehouseName = item.WarehouseName,
            OnHandQty = item.OnHandQty,
            ReservedQty = item.ReservedQty,
            AvailableQty = item.AvailableQty,
        };
}
