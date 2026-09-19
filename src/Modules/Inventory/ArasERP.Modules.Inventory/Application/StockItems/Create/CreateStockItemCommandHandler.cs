using ArasERP.BuildingBlocks.Application;
using ArasERP.Modules.Inventory.Application.Abstractions;
using ArasERP.Modules.Inventory.Domain.StockItems;
using ArasERP.Modules.Inventory.Domain.StockItems.ValueObjects;
using FluentValidation;
using ValidationException = ArasERP.BuildingBlocks.Application.ValidationException;

namespace ArasERP.Modules.Inventory.Application.StockItems.Create;

public sealed class CreateStockItemCommandHandler(
    IStockItemRepository stockItemRepository,
    IValidator<CreateStockItemCommand> validator
) : ICommandHandler<CreateStockItemCommand>
{
    public async Task Handle(
        CreateStockItemCommand command,
        CancellationToken cancellationToken = default
    )
    {
        var validationResult = await validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw new ValidationException([.. validationResult.Errors.Select(e => e.ErrorMessage)]);
        }

        var isSkuExists = await stockItemRepository.ExistsBySkuAsync(
            command.Sku,
            null,
            cancellationToken
        );
        if (isSkuExists)
        {
            throw new ValidationException($"A Stock item with SKU '{command.Sku}' already exists");
        }

        var costingMethod = CostingMethod.FromValue(command.CostingMethod);

        var stockItem = StockItem.Create(
            StockItemId.New(),
            command.Name,
            command.Sku,
            command.Unit,
            costingMethod
        );

        await stockItemRepository.AddAsync(stockItem, cancellationToken);
    }
}
