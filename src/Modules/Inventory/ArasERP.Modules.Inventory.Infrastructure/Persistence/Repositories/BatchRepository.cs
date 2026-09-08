using ArasERP.Modules.Inventory.Application.Abstractions;
using ArasERP.Modules.Inventory.Domain.Batches;
using Microsoft.EntityFrameworkCore;

namespace ArasERP.Modules.Inventory.Infrastructure.Persistence.Repositories;

public sealed class BatchRepository(InventoryDbContext dbContext) : IBatchRepository
{
    public async Task AddAsync(Batch batch, CancellationToken cancellationToken = default)
    {
        await dbContext.Batches.AddAsync(batch, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> ExistsByReceiptNumberAsync(
        string receiptNumber,
        CancellationToken cancellationToken = default
    )
    {
        return await dbContext.Batches.AnyAsync(
            b => b.ReceiptNumber == receiptNumber,
            cancellationToken
        );
    }
}
