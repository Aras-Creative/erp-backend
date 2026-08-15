using ArasERP.BuildingBlocks.Application;
using ArasERP.Modules.Inventory.Application.Abstractions;
using ArasERP.Modules.Inventory.Domain.Warehouses;
using FluentValidation;
using FluentValidation.Results;

namespace ArasERP.Modules.Inventory.Application.Warehouses.Delete;

public sealed class DeleteWarehouseCommandHandler : ICommandHandler<DeleteWarehouseCommand>
{
    private readonly IWarehouseRepository _warehouseRepository;
    private readonly IValidator<DeleteWarehouseCommand> _validator;

    public DeleteWarehouseCommandHandler(
        IWarehouseRepository warehouseRepository,
        IValidator<DeleteWarehouseCommand> validator
    )
    {
        _warehouseRepository = warehouseRepository;
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
            throw new ValidationException(validationResult.Errors);
        }

        var warehouseId = new WarehouseId(Guid.Parse(command.WarehouseId));

        var warehouse =
            await _warehouseRepository.GetByIdAsync(warehouseId, cancellationToken)
            ?? throw new ValidationException(
                new[]
                {
                    new ValidationFailure(
                        nameof(DeleteWarehouseCommand.WarehouseId),
                        $"Warehouse with id '{command.WarehouseId}' was not found."
                    ),
                }
            );

        warehouse.Delete();

        await _warehouseRepository.UpdateAsync(warehouse, cancellationToken);
    }
}
