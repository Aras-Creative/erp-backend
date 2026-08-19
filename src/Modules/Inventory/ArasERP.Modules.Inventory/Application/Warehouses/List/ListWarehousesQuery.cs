using ArasERP.BuildingBlocks.Application;

namespace ArasERP.Modules.Inventory.Application.Warehouses.List;

public sealed record ListWarehousesQuery(
    int Page = PaginationDefaults.Page,
    int PageSize = PaginationDefaults.PageSize
) : IQuery<PagedList<WarehouseListItemDto>>;
