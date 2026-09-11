using ArasERP.Modules.Inventory.Domain.Batches;

namespace ArasERP.Modules.Inventory.Application.Abstractions;

public interface IBatchRepository
{
    Task AddAsync(Batch batch, CancellationToken cancellationToken = default);
}
