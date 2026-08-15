using ArasERP.Modules.Inventory.Application.Warehouses.Create;
using ArasERP.Modules.Inventory.Application.Warehouses.Update;
using ArasERP.Modules.Inventory.Contracts.Warehouses;

namespace ArasERP.Modules.Inventory.Api.Warehouses;

public static class WarehouseRequestMappings
{
    public static CreateWarehouseCommand ToCommand(this CreateWarehouseRequest request) =>
        new()
        {
            Name = request.Name,
            Address = new CreateWarehouseCommand.AddressData
            {
                Street = request.Address.Street,
                City = request.Address.City,
                State = request.Address.State,
                PostalCode = request.Address.PostalCode,
                Country = request.Address.Country,
            },
            PersonInCharge = new CreateWarehouseCommand.PersonInChargeData
            {
                Name = request.PersonInCharge.Name,
                Phone = request.PersonInCharge.Phone,
            },
            FullAddressText = request.FullAddressText,
        };

    public static UpdateWarehouseCommand ToCommand(
        this UpdateWarehouseRequest request,
        Guid warehouseId
    ) =>
        new()
        {
            WarehouseId = warehouseId.ToString(),
            Name = request.Name,
            Address = new UpdateWarehouseCommand.AddressData
            {
                Street = request.Address.Street,
                City = request.Address.City,
                State = request.Address.State,
                PostalCode = request.Address.PostalCode,
                Country = request.Address.Country,
            },
            PersonInCharge = new UpdateWarehouseCommand.PersonInChargeData
            {
                Name = request.PersonInCharge.Name,
                Phone = request.PersonInCharge.Phone,
            },
            FullAddressText = request.FullAddressText,
        };
}
