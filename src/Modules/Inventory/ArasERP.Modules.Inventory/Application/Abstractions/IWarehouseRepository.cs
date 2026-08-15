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
    Task AddAsync(Warehouse warehouse, CancellationToken cancellationToken = default);
    Task UpdateAsync(Warehouse warehouse, CancellationToken cancellationToken = default);
}
