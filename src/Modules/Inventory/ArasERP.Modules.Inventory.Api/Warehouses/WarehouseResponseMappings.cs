using ArasERP.BuildingBlocks.Application;
using ArasERP.Modules.Inventory.Application.Warehouses.GetById;
using ArasERP.Modules.Inventory.Application.Warehouses.GetOptions;
using ArasERP.Modules.Inventory.Application.Warehouses.List;

namespace ArasERP.Modules.Inventory.Api.Warehouses;

public static class WarehouseResponseMappings
{
    public static PagedList<WarehouseResponse> ToResponse(
        this PagedList<WarehouseListItemDto> items
    ) => items.Map(w => w.ToResponse());

    public static WarehouseResponse ToResponse(this WarehouseListItemDto item) =>
        new()
        {
            WarehouseId = item.WarehouseId,
            Name = item.Name,
            PersonInCharge = new WarehouseResponse.PersonInChargeData(
                item.PersonInCharge.Name,
                item.PersonInCharge.Phone
            ),
            Address = new WarehouseResponse.AddressData(
                item.Address.SubDistrictName,
                item.Address.DistrictName,
                item.Address.CityName,
                item.Address.ProvinceName,
                item.Address.ZipCode
            ),
            FullAddressText = item.FullAddressText,
        };

    public static WarehouseResponse ToResponse(this WarehouseDetailDto detail) =>
        new()
        {
            WarehouseId = detail.WarehouseId,
            Name = detail.Name,
            PersonInCharge = new WarehouseResponse.PersonInChargeData(
                detail.PersonInCharge.Name,
                detail.PersonInCharge.Phone
            ),
            Address = new WarehouseResponse.AddressData(
                detail.Address.SubDistrictName,
                detail.Address.DistrictName,
                detail.Address.CityName,
                detail.Address.ProvinceName,
                detail.Address.ZipCode
            ),
            FullAddressText = detail.FullAddressText,
        };

    public static WarehouseOptionResponse ToResponse(this WarehouseOptionDto option) =>
        new(option.WarehouseId, option.Name);
}
