namespace ArasERP.Modules.Inventory.Application.Abstractions;

public interface IInventoryUnitOfWork
{
    Task ExecuteInTransactionAsync(
        Func<Task> action,
        CancellationToken cancellationToken = default
    );
}
