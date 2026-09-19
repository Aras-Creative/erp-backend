using ArasERP.Modules.Inventory.Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace ArasERP.Modules.Inventory.Infrastructure.Persistence;

public sealed class InventoryUnitOfWork(InventoryDbContext dbContext) : IInventoryUnitOfWork
{
    public async Task ExecuteInTransactionAsync(
        Func<Task> action,
        CancellationToken cancellationToken = default
    )
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(
            cancellationToken
        );
        await action();
        await transaction.CommitAsync(cancellationToken);
    }
}
