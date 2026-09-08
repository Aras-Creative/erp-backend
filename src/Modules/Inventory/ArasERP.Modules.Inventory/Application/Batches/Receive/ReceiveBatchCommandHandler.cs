using ArasERP.BuildingBlocks.Application;
using ArasERP.Modules.Inventory.Application.Abstractions;
using ArasERP.Modules.Inventory.Domain.Batches;
using ArasERP.Modules.Inventory.Domain.StockItems;
using ArasERP.Modules.Inventory.Domain.Warehouses;
using FluentValidation;
using ValidationException = ArasERP.BuildingBlocks.Application.ValidationException;

namespace ArasERP.Modules.Inventory.Application.Batches.Receive;

public sealed class ReceiveBatchCommandHandler(
    IBatchRepository batchRepository,
    IStockItemRepository stockItemRepository,
    IWarehouseRepository warehouseRepository,
    IValidator<ReceiveBatchCommand> validator
) : ICommandHandler<ReceiveBatchCommand>
{
    public async Task Handle(
        ReceiveBatchCommand command,
        CancellationToken cancellationToken = default
    )
    {
        var validationResult = await validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw new ValidationException([.. validationResult.Errors.Select(e => e.ErrorMessage)]);
        }

        var isReceiptExists = await batchRepository.ExistsByReceiptNumberAsync(
            command.ReceiptNumber,
            cancellationToken
        );
        if (isReceiptExists)
        {
            throw new ValidationException(
                $"A batch with receipt number '{command.ReceiptNumber}' already exists."
            );
        }

        var itemId = new StockItemId(command.ItemId);
        var isItemActive = await stockItemRepository.IsActiveAsync(itemId, cancellationToken);
        if (!isItemActive)
        {
            throw new ValidationException(
                $"Stock item with id '{command.ItemId}' does not exist or is not active."
            );
        }

        var warehouseId = new WarehouseId(command.WarehouseId);
        var isWarehouseActive = await warehouseRepository.IsActiveAsync(
            warehouseId,
            cancellationToken
        );
        if (!isWarehouseActive)
        {
            throw new ValidationException(
                $"Warehouse with id '{command.WarehouseId}' does not exist or is not active."
            );
        }

        var batch = Batch.Create(
            itemId,
            warehouseId,
            command.ReceivedAt,
            command.ReceivedQty,
            command.UnitCost,
            command.ReceiptNumber,
            command.RecordedBy
        );

        await batchRepository.AddAsync(batch, cancellationToken);
    }
}
