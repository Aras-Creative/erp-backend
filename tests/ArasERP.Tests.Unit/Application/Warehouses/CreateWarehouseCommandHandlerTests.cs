using ArasERP.Modules.Inventory.Application.Abstractions;
using ArasERP.Modules.Inventory.Application.Warehouses.Create;
using ArasERP.Modules.Inventory.Domain.Warehouses;
using FluentAssertions;
using FluentValidation;
using NSubstitute;

namespace ArasERP.Tests.Unit.Application.Warehouses;

public class CreateWarehouseCommandHandlerTests
{
    private readonly IWarehouseRepository _repository = Substitute.For<IWarehouseRepository>();
    private readonly CreateWarehouseCommandValidator _validator = new();
    private readonly CreateWarehouseCommandHandler _sut;

    public CreateWarehouseCommandHandlerTests()
    {
        _sut = new CreateWarehouseCommandHandler(_repository, _validator);
    }

    private static CreateWarehouseCommand CreateCommand(string? name = "Gudang Utama") =>
        new()
        {
            Name = name!,
            Address = new CreateWarehouseCommand.AddressData
            {
                Street = "Jl. Merdeka 1",
                City = "Jakarta",
                State = "DKI Jakarta",
                PostalCode = "10110",
                Country = "Indonesia",
            },
            PersonInCharge = new CreateWarehouseCommand.PersonInChargeData
            {
                Name = "Budi",
                Phone = "08123456789",
            },
            FullAddressText = "Sebelah pasar",
        };

    [Fact]
    public async Task Handle_WithValidCommand_PersistsWarehouse()
    {
        var command = CreateCommand();

        await _sut.Handle(command, CancellationToken.None);

        await _repository
            .Received(1)
            .AddAsync(
                Arg.Is<Warehouse>(w =>
                    w.Name == command.Name
                    && w.PersonInCharge.Name == command.PersonInCharge.Name
                    && w.PersonInCharge.Phone == command.PersonInCharge.Phone
                    && w.Address.Street == command.Address.Street
                    && w.Address.City == command.Address.City
                    && w.Address.State == command.Address.State
                    && w.Address.PostalCode == command.Address.PostalCode
                    && w.Address.Country == command.Address.Country
                    && w.FullAddressText == command.FullAddressText
                ),
                Arg.Any<CancellationToken>()
            );
    }

    [Fact]
    public async Task Handle_WithValidCommand_AddsWarehouseWithDomainEvent()
    {
        Warehouse? captured = null;
        await _repository.AddAsync(
            Arg.Do<Warehouse>(w => captured = w),
            Arg.Any<CancellationToken>()
        );

        await _sut.Handle(CreateCommand(), CancellationToken.None);

        captured.Should().NotBeNull();
        captured!.DomainEvents.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Handle_WithInvalidCommand_ThrowsValidationException()
    {
        var command = CreateCommand("");

        var act = async () => await _sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Handle_WithInvalidCommand_DoesNotPersistWarehouse()
    {
        var command = new CreateWarehouseCommand
        {
            Name = "Gudang Utama",
            Address = new CreateWarehouseCommand.AddressData
            {
                Street = "Jl. Merdeka 1",
                City = "Jakarta",
                State = "DKI Jakarta",
                PostalCode = "10110",
            },
            PersonInCharge = new CreateWarehouseCommand.PersonInChargeData { Name = " " },
        };

        var act = async () => await _sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
        await _repository.DidNotReceiveWithAnyArgs().AddAsync(default!, default);
    }

    [Fact]
    public async Task Handle_WithDuplicateName_ThrowsValidationExceptionAndDoesNotPersist()
    {
        var command = CreateCommand();
        _repository
            .ExistsByNameAsync(command.Name, null, Arg.Any<CancellationToken>())
            .Returns(true);

        var act = async () => await _sut.Handle(command, CancellationToken.None);

        var exception = await act.Should().ThrowAsync<ValidationException>();
        exception
            .Which.Errors.Should()
            .ContainSingle(e => e.PropertyName == nameof(CreateWarehouseCommand.Name));
        await _repository.DidNotReceiveWithAnyArgs().AddAsync(default!, default);
    }
}
