using ArasERP.BuildingBlocks.Application;
using ArasERP.Modules.Inventory.Application.Warehouses.GetById;
using ArasERP.Modules.Inventory.Application.Warehouses.GetOptions;
using ArasERP.Modules.Inventory.Application.Warehouses.List;
using ArasERP.Modules.Inventory.Domain.Warehouses;

namespace ArasERP.Modules.Inventory.Application.Abstractions;

public interface IWarehouseRepository
{
    Task<bool> ExistsByNameAsync(
        string name,
        WarehouseId? excludeId = null,
        CancellationToken cancellationToken = default
    );
    Task<Warehouse?> GetByIdAsync(WarehouseId id, CancellationToken cancellationToken = default);
    Task<bool> IsActiveAsync(WarehouseId id, CancellationToken cancellationToken = default);
    Task<WarehouseDetailDto?> GetDetailAsync(
        Guid warehouseId,
        CancellationToken cancellationToken = default
    );
    Task<PagedList<WarehouseListItemDto>> GetPagedAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default
    );
    Task<IReadOnlyList<WarehouseOptionDto>> GetOptionsAsync(
        CancellationToken cancellationToken = default
    );
    Task AddAsync(Warehouse warehouse, CancellationToken cancellationToken = default);
    Task UpdateAsync(Warehouse warehouse, CancellationToken cancellationToken = default);
}
