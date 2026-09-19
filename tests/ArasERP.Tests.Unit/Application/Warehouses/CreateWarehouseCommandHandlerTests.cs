using ArasERP.BuildingBlocks.Application;
using ArasERP.Modules.AddressClient;
using ArasERP.Modules.AddressClient.Dtos;
using ArasERP.Modules.Inventory.Application.Abstractions;
using ArasERP.Modules.Inventory.Application.Warehouses.Create;
using ArasERP.Modules.Inventory.Domain.Warehouses;
using FluentAssertions;
using NSubstitute;

namespace ArasERP.Tests.Unit.Application.Warehouses;

public class CreateWarehouseCommandHandlerTests
{
    private readonly IWarehouseRepository _repository = Substitute.For<IWarehouseRepository>();
    private readonly IAddressClient _addressClient = Substitute.For<IAddressClient>();
    private readonly CreateWarehouseCommandValidator _validator = new();
    private readonly CreateWarehouseCommandHandler _sut;

    private static readonly Guid SampleAddressId = Guid.NewGuid();

    public CreateWarehouseCommandHandlerTests()
    {
        _sut = new CreateWarehouseCommandHandler(_addressClient, _repository, _validator);

        _addressClient
            .GetByIdAsync(SampleAddressId, Arg.Any<CancellationToken>())
            .Returns(
                new AddressDto
                {
                    AddressId = SampleAddressId,
                    ProvinceName = "DKI Jakarta",
                    CityName = "Jakarta",
                    DistrictName = "Menteng",
                    SubDistrictName = "Kebon Sirih",
                    ZipCode = "10110",
                }
            );
    }

    private static CreateWarehouseCommand CreateCommand(
        string? name = "Gudang Utama",
        Guid? addressId = null
    ) =>
        new()
        {
            Name = name!,
            AddressId = addressId ?? SampleAddressId,
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
                    && w.Address.SubDistrictName == "Kebon Sirih"
                    && w.Address.DistrictName == "Menteng"
                    && w.Address.CityName == "Jakarta"
                    && w.Address.ProvinceName == "DKI Jakarta"
                    && w.Address.ZipCode == "10110"
                    && w.FullAddressText == command.FullAddressText
                    && w.Address.AddressId == SampleAddressId
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
            AddressId = SampleAddressId,
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
        exception.Which.Errors.Should().ContainSingle(e => e.Contains("already exists"));
        await _repository.DidNotReceiveWithAnyArgs().AddAsync(default!, default);
    }

    [Fact]
    public async Task Handle_WithInvalidAddressId_ThrowsValidationException()
    {
        _addressClient
            .GetByIdAsync(SampleAddressId, Arg.Any<CancellationToken>())
            .Returns((AddressDto?)null);

        var command = CreateCommand();

        var act = async () => await _sut.Handle(command, CancellationToken.None);

        var exception = await act.Should().ThrowAsync<ValidationException>();
        exception.Which.Errors.Should().ContainSingle(e => e.Contains("address"));
    }
}
