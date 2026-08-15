using ArasERP.BuildingBlocks.Application;

namespace ArasERP.Modules.Inventory.Application.Warehouses.GetById;

public sealed record GetWarehouseByIdQuery(Guid WarehouseId) : IQuery<WarehouseDetailDto?>;
