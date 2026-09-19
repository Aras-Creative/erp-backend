using ArasERP.BuildingBlocks.Application;
using ArasERP.Modules.Inventory.Application.Abstractions;
using ArasERP.Modules.Inventory.Application.Warehouses.Delete;
using ArasERP.Modules.Inventory.Domain.Warehouses;
using ArasERP.Modules.Inventory.Domain.Warehouses.ValueObjects;
using FluentAssertions;
using FluentValidation;
using NSubstitute;
using Xunit;
using ValidationException = ArasERP.BuildingBlocks.Application.ValidationException;

namespace ArasERP.Tests.Unit.Application.Warehouses;

public class DeleteWarehouseCommandHandlerTests
{
    private readonly IWarehouseRepository _repository = Substitute.For<IWarehouseRepository>();
    private readonly IStockLevelRepository _stockLevelRepository =
        Substitute.For<IStockLevelRepository>();
    private readonly IValidator<DeleteWarehouseCommand> _validator =
        new DeleteWarehouseCommandValidator();
    private readonly DeleteWarehouseCommandHandler _sut;

    public DeleteWarehouseCommandHandlerTests()
    {
        _sut = new DeleteWarehouseCommandHandler(_repository, _stockLevelRepository, _validator);
        _stockLevelRepository
            .HasStockAsync(Arg.Any<WarehouseId>(), Arg.Any<CancellationToken>())
            .Returns(false);
    }

    [Fact]
    public async Task Handle_WithExistingWarehouse_SoftDeletesAndPersists()
    {
        var warehouse = CreateWarehouse();
        var command = new DeleteWarehouseCommand { WarehouseId = warehouse.Id.Value.ToString() };
        _repository.GetByIdAsync(warehouse.Id, Arg.Any<CancellationToken>()).Returns(warehouse);

        await _sut.Handle(command, CancellationToken.None);

        warehouse.IsDeleted.Should().BeTrue();
        warehouse.DeletedAtUtc.Should().NotBeNull();
        await _repository.Received(1).UpdateAsync(warehouse, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenWarehouseHasStock_ThrowsAndDoesNotDelete()
    {
        var warehouse = CreateWarehouse();
        var command = new DeleteWarehouseCommand { WarehouseId = warehouse.Id.Value.ToString() };
        _repository.GetByIdAsync(warehouse.Id, Arg.Any<CancellationToken>()).Returns(warehouse);
        _stockLevelRepository
            .HasStockAsync(warehouse.Id, Arg.Any<CancellationToken>())
            .Returns(true);

        var act = async () => await _sut.Handle(command, CancellationToken.None);

        var exception = await act.Should().ThrowAsync<ValidationException>();
        exception.Which.Errors.Should().ContainSingle(e => e.Contains("has stock"));
        warehouse.IsDeleted.Should().BeFalse();
        await _repository.DidNotReceiveWithAnyArgs().UpdateAsync(default!, default);
    }

    [Fact]
    public async Task Handle_WhenWarehouseNotFound_ThrowsValidationException()
    {
        var warehouseId = WarehouseId.New();
        var command = new DeleteWarehouseCommand { WarehouseId = warehouseId.Value.ToString() };
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
        var command = new DeleteWarehouseCommand { WarehouseId = "not-a-guid" };

        var act = async () => await _sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
        await _repository.DidNotReceiveWithAnyArgs().UpdateAsync(default!, default);
    }

    private static Warehouse CreateWarehouse()
    {
        return Warehouse.Create(
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
    }
}
