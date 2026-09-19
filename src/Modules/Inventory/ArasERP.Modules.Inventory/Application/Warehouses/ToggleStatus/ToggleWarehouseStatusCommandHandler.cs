using ArasERP.BuildingBlocks.Application;
using ArasERP.Modules.Inventory.Application.Abstractions;
using ArasERP.Modules.Inventory.Domain.Warehouses;
using FluentValidation;
using ValidationException = ArasERP.BuildingBlocks.Application.ValidationException;

namespace ArasERP.Modules.Inventory.Application.Warehouses.ToggleStatus;

public sealed class ToggleWarehouseStatusCommandHandler
    : ICommandHandler<ToggleWarehouseStatusCommand>
{
    private readonly IWarehouseRepository _warehouseRepository;
    private readonly IValidator<ToggleWarehouseStatusCommand> _validator;

    public ToggleWarehouseStatusCommandHandler(
        IWarehouseRepository warehouseRepository,
        IValidator<ToggleWarehouseStatusCommand> validator
    )
    {
        _warehouseRepository = warehouseRepository;
        _validator = validator;
    }

    public async Task Handle(
        ToggleWarehouseStatusCommand command,
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

        warehouse.ToggleStatus();

        await _warehouseRepository.UpdateAsync(warehouse, cancellationToken);
    }
}
