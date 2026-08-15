using ArasERP.BuildingBlocks.Application;
using ArasERP.Modules.Inventory.Application.Abstractions;
using ArasERP.Modules.Inventory.Domain.Warehouses;
using ArasERP.Modules.Inventory.Domain.Warehouses.ValueObjects;
using FluentValidation;
using FluentValidation.Results;

namespace ArasERP.Modules.Inventory.Application.Warehouses.Create;

public sealed class CreateWarehouseCommandHandler : ICommandHandler<CreateWarehouseCommand>
{
    private readonly IWarehouseRepository _warehouseRepository;
    private readonly IValidator<CreateWarehouseCommand> _validator;

    public CreateWarehouseCommandHandler(
        IWarehouseRepository warehouseRepository,
        IValidator<CreateWarehouseCommand> validator
    )
    {
        _warehouseRepository = warehouseRepository;
        _validator = validator;
    }

    public async Task Handle(
        CreateWarehouseCommand command,
        CancellationToken cancellationToken = default
    )
    {
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        if (await _warehouseRepository.ExistsByNameAsync(command.Name, null, cancellationToken))
        {
            throw new ValidationException(
                new[]
                {
                    new ValidationFailure(
                        nameof(CreateWarehouseCommand.Name),
                        $"A warehouse with name '{command.Name}' already exists."
                    ),
                }
            );
        }

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

        var warehouse = Warehouse.Create(
            command.Name,
            personInCharge,
            address,
            command.FullAddressText
        );

        await _warehouseRepository.AddAsync(warehouse, cancellationToken);
    }
}
