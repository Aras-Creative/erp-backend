using ArasERP.BuildingBlocks.Application;

namespace ArasERP.Modules.Inventory.Application.Warehouses.GetOptions;

public sealed record GetWarehouseOptionsQuery : IQuery<IReadOnlyList<WarehouseOptionDto>>;
