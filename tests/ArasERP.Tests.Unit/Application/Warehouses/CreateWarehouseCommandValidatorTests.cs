using ArasERP.Modules.Inventory.Application.Warehouses.Create;
using FluentAssertions;

namespace ArasERP.Tests.Unit.Application.Warehouses;

public class CreateWarehouseCommandValidatorTests
{
    private readonly CreateWarehouseCommandValidator _sut = new();

    private static CreateWarehouseCommand.PersonInChargeData CreatePersonInCharge() =>
        new() { Name = "Budi", Phone = "08123456789" };

    private static CreateWarehouseCommand CreateCommand(
        string? name = "Gudang Utama",
        Guid? addressId = null) =>
        new()
        {
            Name = name!,
            AddressId = addressId ?? Guid.NewGuid(),
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
    public void Validate_WithEmptyAddressId_ReturnsError()
    {
        var command = new CreateWarehouseCommand
        {
            Name = "Gudang Utama",
            AddressId = Guid.Empty,
            PersonInCharge = CreatePersonInCharge(),
        };

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .ContainSingle(e => e.PropertyName == nameof(CreateWarehouseCommand.AddressId));
    }

    [Fact]
    public void Validate_WithNullPersonInCharge_ReturnsError()
    {
        var command = new CreateWarehouseCommand
        {
            Name = "Gudang Utama",
            AddressId = Guid.NewGuid(),
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
            AddressId = Guid.NewGuid(),
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
