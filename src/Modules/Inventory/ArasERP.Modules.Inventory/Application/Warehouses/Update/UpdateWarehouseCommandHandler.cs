using ArasERP.BuildingBlocks.Application;
using ArasERP.Modules.Inventory.Application.Abstractions;
using ArasERP.Modules.Inventory.Domain.Warehouses;
using ArasERP.Modules.Inventory.Domain.Warehouses.ValueObjects;
using FluentValidation;
using ValidationException = ArasERP.BuildingBlocks.Application.ValidationException;

namespace ArasERP.Modules.Inventory.Application.Warehouses.Update;

public sealed class UpdateWarehouseCommandHandler : ICommandHandler<UpdateWarehouseCommand>
{
    private readonly IWarehouseRepository _warehouseRepository;
    private readonly IValidator<UpdateWarehouseCommand> _validator;

    public UpdateWarehouseCommandHandler(
        IWarehouseRepository warehouseRepository,
        IValidator<UpdateWarehouseCommand> validator
    )
    {
        _warehouseRepository = warehouseRepository;
        _validator = validator;
    }

    public async Task Handle(
        UpdateWarehouseCommand command,
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

        if (
            await _warehouseRepository.ExistsByNameAsync(
                command.Name,
                warehouseId,
                cancellationToken
            )
        )
        {
            throw new ValidationException(
                $"A warehouse with name '{command.Name}' already exists."
            );
        }

        var warehouse =
            await _warehouseRepository.GetByIdAsync(warehouseId, cancellationToken)
            ?? throw new ValidationException(
                $"Warehouse with id '{command.WarehouseId}' was not found."
            );

        var address = WarehouseAddress.Create(
            command.Address.Street,
            command.Address.City,
            command.Address.State,
            command.Address.PostalCode,
            command.Address.Country
        );

        var personInCharge = WarehousePersonInCharge.Create(
            command.PersonInCharge.Name,
            command.PersonInCharge.Phone
        );

        warehouse.Update(command.Name, personInCharge, address, command.FullAddressText);

        await _warehouseRepository.UpdateAsync(warehouse, cancellationToken);
    }
}
