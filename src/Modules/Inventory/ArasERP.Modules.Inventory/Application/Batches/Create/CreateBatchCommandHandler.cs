using ArasERP.BuildingBlocks.Application;
using ArasERP.Modules.Inventory.Application.Abstractions;
using ArasERP.Modules.Inventory.Domain.Batches;
using ArasERP.Modules.Inventory.Domain.StockItems;
using ArasERP.Modules.Inventory.Domain.StockLevels;
using ArasERP.Modules.Inventory.Domain.StockMovements;
using ArasERP.Modules.Inventory.Domain.Warehouses;
using FluentValidation;
using ValidationException = ArasERP.BuildingBlocks.Application.ValidationException;

namespace ArasERP.Modules.Inventory.Application.Batches.Create;

public sealed class CreateBatchCommandHandler(
    IBatchRepository batchRepository,
    IStockItemRepository stockItemRepository,
    IWarehouseRepository warehouseRepository,
    IStockLevelRepository stockLevelRepository,
    IStockMovementRepository stockMovementRepository,
    IInventoryUnitOfWork unitOfWork,
    IValidator<CreateBatchCommand> validator
) : ICommandHandler<CreateBatchCommand>
{
    public async Task Handle(
        CreateBatchCommand command,
        CancellationToken cancellationToken = default
    )
    {
        var validationResult = await validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw new ValidationException([.. validationResult.Errors.Select(e => e.ErrorMessage)]);
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

        var sourceType = SourceType.FromValue(command.SourceType);
        var direction = sourceType.DefaultDirection;
        var recordedBy = command.RecordedBy;

        await unitOfWork.ExecuteInTransactionAsync(
            async () =>
            {
                var batch = Batch.Create(
                    itemId,
                    warehouseId,
                    command.ReceivedAt,
                    command.ReceivedQty,
                    command.UnitCost
                );

                await batchRepository.AddAsync(batch, cancellationToken);

                var movement = StockMovement.Create(
                    itemId,
                    warehouseId,
                    direction,
                    command.ReceivedQty,
                    sourceType,
                    externalReferenceNo: command.ExternalReferenceNo,
                    batchId: batch.Id,
                    note: command.Note,
                    recordedBy: recordedBy,
                    receivedBy: command.ReceivedBy
                );
                await stockMovementRepository.AddAsync(movement, cancellationToken);

                var stockLevel = await stockLevelRepository.GetByKeyAsync(
                    itemId,
                    warehouseId,
                    cancellationToken
                );
                if (stockLevel is null)
                {
                    stockLevel = StockLevel.Create(itemId, warehouseId);
                    stockLevel.Receive(command.ReceivedQty);
                    await stockLevelRepository.AddAsync(stockLevel, cancellationToken);
                }
                else
                {
                    stockLevel.Receive(command.ReceivedQty);
                    await stockLevelRepository.UpdateAsync(stockLevel, cancellationToken);
                }
            },
            cancellationToken
        );
    }
}