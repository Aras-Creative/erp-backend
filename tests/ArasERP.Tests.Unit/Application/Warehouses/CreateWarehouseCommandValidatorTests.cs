using ArasERP.Modules.Inventory.Application.Warehouses.Create;
using FluentAssertions;

namespace ArasERP.Tests.Unit.Application.Warehouses;

public class CreateWarehouseCommandValidatorTests
{
    private readonly CreateWarehouseCommandValidator _sut = new();

    private static CreateWarehouseCommand.AddressData CreateAddress() =>
        new()
        {
            Street = "Jl. Merdeka 1",
            City = "Jakarta",
            State = "DKI Jakarta",
            PostalCode = "10110",
            Country = "Indonesia",
        };

    private static CreateWarehouseCommand.PersonInChargeData CreatePersonInCharge() =>
        new() { Name = "Budi", Phone = "08123456789" };

    private static CreateWarehouseCommand CreateCommand(string? name = "Gudang Utama") =>
        new()
        {
            Name = name!,
            Address = CreateAddress(),
            PersonInCharge = CreatePersonInCharge(),
            FullAddressText = "Sebelah pasar",
        };

    [Fact]
    public void Validate_WithValidCommand_ReturnsNoErrors()
    {
        var result = _sut.Validate(CreateCommand());

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Validate_WithMissingName_ReturnsError(string? name)
    {
        var result = _sut.Validate(CreateCommand(name));

        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .ContainSingle(e => e.PropertyName == nameof(CreateWarehouseCommand.Name));
    }

    [Fact]
    public void Validate_WithNullAddress_ReturnsError()
    {
        var command = new CreateWarehouseCommand
        {
            Name = "Gudang Utama",
            Address = null!,
            PersonInCharge = CreatePersonInCharge(),
        };

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .ContainSingle(e => e.PropertyName == nameof(CreateWarehouseCommand.Address));
    }

    [Theory]
    [InlineData("Street")]
    [InlineData("City")]
    [InlineData("State")]
    [InlineData("PostalCode")]
    public void Validate_WithMissingAddressComponent_ReturnsError(string component)
    {
        var command = CreateCommand();
        typeof(CreateWarehouseCommand.AddressData)
            .GetProperty(component)!
            .SetValue(command.Address, null);

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .ContainSingle(e =>
                e.PropertyName == $"{nameof(CreateWarehouseCommand.Address)}.{component}"
            );
    }

    [Fact]
    public void Validate_WithNullPersonInCharge_ReturnsError()
    {
        var command = new CreateWarehouseCommand
        {
            Name = "Gudang Utama",
            Address = CreateAddress(),
            PersonInCharge = null!,
        };

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .ContainSingle(e => e.PropertyName == nameof(CreateWarehouseCommand.PersonInCharge));
    }

    [Fact]
    public void Validate_WithMissingPersonInChargeName_ReturnsError()
    {
        var command = new CreateWarehouseCommand
        {
            Name = "Gudang Utama",
            Address = CreateAddress(),
            PersonInCharge = new CreateWarehouseCommand.PersonInChargeData { Name = "" },
        };

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .ContainSingle(e =>
                e.PropertyName == $"{nameof(CreateWarehouseCommand.PersonInCharge)}.Name"
            );
    }
}
