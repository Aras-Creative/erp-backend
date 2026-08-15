using ArasERP.BuildingBlocks.Application;
using ArasERP.Modules.Inventory.Application.Abstractions;

namespace ArasERP.Modules.Inventory.Application.Warehouses.GetById;

public sealed class GetWarehouseByIdQueryHandler
    : IQueryHandler<GetWarehouseByIdQuery, WarehouseDetailDto?>
{
    private readonly IWarehouseRepository _warehouseRepository;

    public GetWarehouseByIdQueryHandler(IWarehouseRepository warehouseRepository)
    {
        _warehouseRepository = warehouseRepository;
    }

    public async Task<WarehouseDetailDto?> Handle(
        GetWarehouseByIdQuery query,
        CancellationToken cancellationToken = default
    )
    {
        return await _warehouseRepository.GetDetailAsync(query.WarehouseId, cancellationToken);
    }
}
