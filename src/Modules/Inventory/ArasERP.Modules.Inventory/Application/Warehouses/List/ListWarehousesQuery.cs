using ArasERP.BuildingBlocks.Application;

namespace ArasERP.Modules.Inventory.Application.Warehouses.List;

public sealed record ListWarehousesQuery : IQuery<IReadOnlyList<WarehouseListItemDto>>;
