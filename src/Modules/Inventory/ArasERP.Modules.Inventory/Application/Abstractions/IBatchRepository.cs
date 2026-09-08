using ArasERP.Modules.Inventory.Domain.Batches;

namespace ArasERP.Modules.Inventory.Application.Abstractions;

public interface IBatchRepository
{
    Task AddAsync(Batch batch, CancellationToken cancellationToken = default);
    Task<bool> ExistsByReceiptNumberAsync(
        string receiptNumber,
        CancellationToken cancellationToken = default
    );
}
