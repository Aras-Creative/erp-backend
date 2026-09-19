using ArasERP.BuildingBlocks.Application;
using ArasERP.Modules.AddressClient;
using ArasERP.Modules.Inventory.Application.Abstractions;
using ArasERP.Modules.Inventory.Domain.Warehouses;
using ArasERP.Modules.Inventory.Domain.Warehouses.ValueObjects;
using FluentValidation;
using ValidationException = ArasERP.BuildingBlocks.Application.ValidationException;

namespace ArasERP.Modules.Inventory.Application.Warehouses.Create;

public sealed class CreateWarehouseCommandHandler(
    IAddressClient addressClient,
    IWarehouseRepository warehouseRepository,
    IValidator<CreateWarehouseCommand> validator
) : ICommandHandler<CreateWarehouseCommand>
{
    public async Task Handle(
        CreateWarehouseCommand command,
        CancellationToken cancellationToken = default
    )
    {
        var validationResult = await validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw new ValidationException([.. validationResult.Errors.Select(e => e.ErrorMessage)]);
        }

        if (await warehouseRepository.ExistsByNameAsync(command.Name, null, cancellationToken))
        {
            throw new ValidationException(
                $"A warehouse with name '{command.Name}' already exists."
            );
        }

        var addressRef = await addressClient.GetByIdAsync(command.AddressId, cancellationToken);
        if (addressRef is null)
        {
            throw new ValidationException($"Warehouse address is invalid");
        }

        var address = WarehouseAddress.Create(
            addressRef.AddressId,
            addressRef.SubDistrictName,
            addressRef.DistrictName,
            addressRef.CityName,
            addressRef.ProvinceName,
            addressRef.ZipCode
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

        await warehouseRepository.AddAsync(warehouse, cancellationToken);
    }
}
