using ArasERP.BuildingBlocks.Application;
using ArasERP.Modules.Inventory.Application.Abstractions;

namespace ArasERP.Modules.Inventory.Application.Warehouses.GetOptions;

public sealed class GetWarehouseOptionsQueryHandler
    : IQueryHandler<GetWarehouseOptionsQuery, IReadOnlyList<WarehouseOptionDto>>
{
    private readonly IWarehouseRepository _warehouseRepository;

    public GetWarehouseOptionsQueryHandler(IWarehouseRepository warehouseRepository)
    {
        _warehouseRepository = warehouseRepository;
    }

    public async Task<IReadOnlyList<WarehouseOptionDto>> Handle(
        GetWarehouseOptionsQuery query,
        CancellationToken cancellationToken = default
    )
    {
        return await _warehouseRepository.GetOptionsAsync(cancellationToken);
    }
}
