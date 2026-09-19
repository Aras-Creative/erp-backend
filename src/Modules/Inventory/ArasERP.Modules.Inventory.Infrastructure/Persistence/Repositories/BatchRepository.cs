using ArasERP.Modules.Inventory.Application.Abstractions;
using ArasERP.Modules.Inventory.Domain.Batches;

namespace ArasERP.Modules.Inventory.Infrastructure.Persistence.Repositories;

public sealed class BatchRepository(InventoryDbContext dbContext) : IBatchRepository
{
    public async Task AddAsync(Batch batch, CancellationToken cancellationToken = default)
    {
        await dbContext.Batches.AddAsync(batch, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
