using ArasERP.BuildingBlocks.Application;
using ArasERP.Modules.AddressClient;
using ArasERP.Modules.AddressClient.Dtos;
using ArasERP.Modules.Inventory.Application.Abstractions;
using ArasERP.Modules.Inventory.Application.Warehouses.Update;
using ArasERP.Modules.Inventory.Domain.Warehouses;
using ArasERP.Modules.Inventory.Domain.Warehouses.ValueObjects;
using FluentAssertions;
using FluentValidation;
using NSubstitute;
using Xunit;
using ValidationException = ArasERP.BuildingBlocks.Application.ValidationException;

namespace ArasERP.Tests.Unit.Application.Warehouses;

public class UpdateWarehouseCommandHandlerTests
{
    private readonly IAddressClient _addressClient = Substitute.For<IAddressClient>();
    private readonly IWarehouseRepository _repository = Substitute.For<IWarehouseRepository>();
    private readonly IValidator<UpdateWarehouseCommand> _validator =
        new UpdateWarehouseCommandValidator();
    private readonly UpdateWarehouseCommandHandler _sut;

    public UpdateWarehouseCommandHandlerTests()
    {
        _sut = new UpdateWarehouseCommandHandler(_addressClient, _repository, _validator);
    }

    [Fact]
    public async Task Handle_WithValidCommand_UpdatesWarehouse()
    {
        var warehouse = CreateWarehouse();
        var addressId = Guid.NewGuid();
        var command = CreateCommand(warehouse.Id.Value.ToString(), addressId);
        _repository.GetByIdAsync(warehouse.Id, Arg.Any<CancellationToken>()).Returns(warehouse);
        _addressClient
            .GetByIdAsync(addressId, Arg.Any<CancellationToken>())
            .Returns(
                new AddressDto
                {
                    AddressId = addressId,
                    DestinationCode = "CGK10302",
                    OriginCode = "CGK10000",
                    ProvinceName = "DKI JAKARTA",
                    CityName = "JAKARTA PUSAT",
                    DistrictName = "GAMBIR",
                    SubDistrictName = "GAMBIR",
                    ZipCode = "10110",
                }
            );

        await _sut.Handle(command, CancellationToken.None);

        warehouse.Name.Should().Be(command.Name);
        warehouse.Address.SubDistrictName.Should().Be("GAMBIR");
        warehouse.PersonInCharge.Name.Should().Be(command.PersonInCharge.Name);
        warehouse.FullAddressText.Should().Be(command.FullAddressText);
        await _repository.Received(1).UpdateAsync(warehouse, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithDuplicateName_ThrowsValidationExceptionAndDoesNotUpdate()
    {
        var warehouse = CreateWarehouse();
        var addressId = Guid.NewGuid();
        var command = CreateCommand(warehouse.Id.Value.ToString(), addressId);
        _repository
            .ExistsByNameAsync(command.Name, warehouse.Id, Arg.Any<CancellationToken>())
            .Returns(true);

        var act = async () => await _sut.Handle(command, CancellationToken.None);

        var exception = await act.Should().ThrowAsync<ValidationException>();
        exception.Which.Errors.Should().ContainSingle(e => e.Contains("already exists"));
        await _repository.DidNotReceiveWithAnyArgs().UpdateAsync(default!, default);
    }

    [Fact]
    public async Task Handle_WhenWarehouseNotFound_ThrowsValidationException()
    {
        var warehouse = CreateWarehouse();
        var addressId = Guid.NewGuid();
        var command = CreateCommand(warehouse.Id.Value.ToString(), addressId);
        _repository
            .GetByIdAsync(warehouse.Id, Arg.Any<CancellationToken>())
            .Returns((Warehouse?)null);

        var act = async () => await _sut.Handle(command, CancellationToken.None);

        var exception = await act.Should().ThrowAsync<ValidationException>();
        exception.Which.Errors.Should().ContainSingle(e => e.Contains("was not found"));
        await _repository.DidNotReceiveWithAnyArgs().UpdateAsync(default!, default);
    }

    [Fact]
    public async Task Handle_WithInvalidCommand_ThrowsValidationException()
    {
        var command = CreateCommand("not-a-guid");

        var act = async () => await _sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
        await _repository.DidNotReceiveWithAnyArgs().UpdateAsync(default!, default);
    }

    private static Warehouse CreateWarehouse()
    {
        return Warehouse.Create(
            "Gudang Lama",
            WarehousePersonInCharge.Create("Budi"),
            WarehouseAddress.Create("Kebon Sirih", "Menteng", "Jakarta", "DKI Jakarta", "10110"),
            "Gudang Lama"
        );
    }

    private static UpdateWarehouseCommand CreateCommand(string warehouseId, Guid? addressId = null)
    {
        return new UpdateWarehouseCommand
        {
            WarehouseId = warehouseId,
            Name = "Gudang Baru",
            AddressId = addressId ?? Guid.NewGuid(),
            PersonInCharge = new UpdateWarehouseCommand.PersonInChargeData { Name = "Andi" },
            FullAddressText = "Gudang Baru",
        };
    }
}
