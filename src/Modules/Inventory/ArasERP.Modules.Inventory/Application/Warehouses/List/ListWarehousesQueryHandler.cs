using ArasERP.BuildingBlocks.Application;
using ArasERP.Modules.Inventory.Application.Abstractions;

namespace ArasERP.Modules.Inventory.Application.Warehouses.List;

public sealed class ListWarehousesQueryHandler
    : IQueryHandler<ListWarehousesQuery, PagedList<WarehouseListItemDto>>
{
    private readonly IWarehouseRepository _warehouseRepository;

    public ListWarehousesQueryHandler(IWarehouseRepository warehouseRepository)
    {
        _warehouseRepository = warehouseRepository;
    }

    public async Task<PagedList<WarehouseListItemDto>> Handle(
        ListWarehousesQuery query,
        CancellationToken cancellationToken = default
    )
    {
        return await _warehouseRepository.GetPagedAsync(
            query.Page,
            query.PageSize,
            cancellationToken
        );
    }
}
