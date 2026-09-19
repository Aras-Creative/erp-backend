using ArasERP.BuildingBlocks.Application;
using ArasERP.Modules.AddressClient;
using ArasERP.Modules.Inventory.Application.Abstractions;
using ArasERP.Modules.Inventory.Domain.Warehouses;
using ArasERP.Modules.Inventory.Domain.Warehouses.ValueObjects;
using FluentValidation;
using ValidationException = ArasERP.BuildingBlocks.Application.ValidationException;

namespace ArasERP.Modules.Inventory.Application.Warehouses.Update;

public sealed class UpdateWarehouseCommandHandler(
    IAddressClient addressClient,
    IWarehouseRepository warehouseRepository,
    IValidator<UpdateWarehouseCommand> validator
) : ICommandHandler<UpdateWarehouseCommand>
{
    public async Task Handle(
        UpdateWarehouseCommand command,
        CancellationToken cancellationToken = default
    )
    {
        var validationResult = await validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(
                validationResult.Errors.Select(e => e.ErrorMessage).ToList()
            );
        }

        var warehouseId = new WarehouseId(Guid.Parse(command.WarehouseId));

        if (
            await warehouseRepository.ExistsByNameAsync(
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
            await warehouseRepository.GetByIdAsync(warehouseId, cancellationToken)
            ?? throw new ValidationException(
                $"Warehouse with id '{command.WarehouseId}' was not found."
            );

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

        warehouse.Update(command.Name, personInCharge, address, command.FullAddressText);

        await warehouseRepository.UpdateAsync(warehouse, cancellationToken);
    }
}
