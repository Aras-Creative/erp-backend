using ArasERP.BuildingBlocks.Application;
using ArasERP.Modules.Inventory.Application.Abstractions;
using ArasERP.Modules.Inventory.Domain.Warehouses;
using FluentValidation;
using ValidationException = ArasERP.BuildingBlocks.Application.ValidationException;

namespace ArasERP.Modules.Inventory.Application.Warehouses.Delete;

public sealed class DeleteWarehouseCommandHandler : ICommandHandler<DeleteWarehouseCommand>
{
    private readonly IWarehouseRepository _warehouseRepository;
    private readonly IStockLevelRepository _stockLevelRepository;
    private readonly IValidator<DeleteWarehouseCommand> _validator;

    public DeleteWarehouseCommandHandler(
        IWarehouseRepository warehouseRepository,
        IStockLevelRepository stockLevelRepository,
        IValidator<DeleteWarehouseCommand> validator
    )
    {
        _warehouseRepository = warehouseRepository;
        _stockLevelRepository = stockLevelRepository;
        _validator = validator;
    }

    public async Task Handle(
        DeleteWarehouseCommand command,
        CancellationToken cancellationToken = default
    )
    {
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(
                validationResult.Errors.Select(e => e.ErrorMessage).ToList()
            );
        }

        var warehouseId = new WarehouseId(Guid.Parse(command.WarehouseId));

        var warehouse =
            await _warehouseRepository.GetByIdAsync(warehouseId, cancellationToken)
            ?? throw new ValidationException(
                $"Warehouse with id '{command.WarehouseId}' was not found."
            );

        var hasStock = await _stockLevelRepository.HasStockAsync(warehouse.Id, cancellationToken);
        if (hasStock)
        {
            throw new ValidationException(
                $"Warehouse with id '{command.WarehouseId}' cannot be deleted because it has stock."
            );
        }

        warehouse.Delete();

        await _warehouseRepository.UpdateAsync(warehouse, cancellationToken);
    }
}
