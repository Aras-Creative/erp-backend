using ArasERP.BuildingBlocks.Application;
using ArasERP.Modules.Inventory.Application.Abstractions;
using ArasERP.Modules.Inventory.Application.Warehouses.ToggleStatus;
using ArasERP.Modules.Inventory.Domain.Warehouses;
using ArasERP.Modules.Inventory.Domain.Warehouses.ValueObjects;
using FluentAssertions;
using FluentValidation;
using NSubstitute;
using Xunit;
using ValidationException = ArasERP.BuildingBlocks.Application.ValidationException;

namespace ArasERP.Tests.Unit.Application.Warehouses;

public class ToggleWarehouseStatusCommandHandlerTests
{
    private readonly IWarehouseRepository _repository = Substitute.For<IWarehouseRepository>();
    private readonly IValidator<ToggleWarehouseStatusCommand> _validator =
        new ToggleWarehouseStatusCommandValidator();
    private readonly ToggleWarehouseStatusCommandHandler _sut;

    public ToggleWarehouseStatusCommandHandlerTests()
    {
        _sut = new ToggleWarehouseStatusCommandHandler(_repository, _validator);
    }

    [Fact]
    public async Task Handle_WithActiveWarehouse_DeactivatesAndPersists()
    {
        var warehouse = CreateWarehouse();
        var command = new ToggleWarehouseStatusCommand
        {
            WarehouseId = warehouse.Id.Value.ToString(),
        };
        _repository.GetByIdAsync(warehouse.Id, Arg.Any<CancellationToken>()).Returns(warehouse);

        await _sut.Handle(command, CancellationToken.None);

        warehouse.IsActive.Should().BeFalse();
        await _repository.Received(1).UpdateAsync(warehouse, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithInactiveWarehouse_ActivatesAndPersists()
    {
        var warehouse = CreateWarehouse();
        warehouse.ToggleStatus();
        var command = new ToggleWarehouseStatusCommand
        {
            WarehouseId = warehouse.Id.Value.ToString(),
        };
        _repository.GetByIdAsync(warehouse.Id, Arg.Any<CancellationToken>()).Returns(warehouse);

        await _sut.Handle(command, CancellationToken.None);

        warehouse.IsActive.Should().BeTrue();
        await _repository.Received(1).UpdateAsync(warehouse, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenWarehouseNotFound_ThrowsValidationException()
    {
        var warehouseId = WarehouseId.New();
        var command = new ToggleWarehouseStatusCommand
        {
            WarehouseId = warehouseId.Value.ToString(),
        };
        _repository
            .GetByIdAsync(warehouseId, Arg.Any<CancellationToken>())
            .Returns((Warehouse?)null);

        var act = async () => await _sut.Handle(command, CancellationToken.None);

        var exception = await act.Should().ThrowAsync<ValidationException>();
        exception.Which.Errors.Should().ContainSingle(e => e.Contains("was not found"));
        await _repository.DidNotReceiveWithAnyArgs().UpdateAsync(default!, default);
    }

    [Fact]
    public async Task Handle_WithInvalidCommand_ThrowsValidationException()
    {
        var command = new ToggleWarehouseStatusCommand { WarehouseId = "not-a-guid" };

        var act = async () => await _sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
        await _repository.DidNotReceiveWithAnyArgs().UpdateAsync(default!, default);
    }

    private static Warehouse CreateWarehouse()
    {
        var warehouse = Warehouse.Create(
            "Gudang Utama",
            WarehousePersonInCharge.Create("Budi"),
            WarehouseAddress.Create(
                Guid.NewGuid(),
                "Kebon Sirih",
                "Menteng",
                "Jakarta",
                "DKI Jakarta",
                "10110"
            ),
            "Sebelah pasar"
        );
        warehouse.ClearDomainEvents();
        return warehouse;
    }
}
