using ArasERP.Modules.Inventory.Application.StockItems.Create;
using FluentAssertions;

namespace ArasERP.Tests.Unit.Application.StockItems.Create;

public class CreateStockItemValidatorTests
{
    private readonly CreateStockItemValidator _sut = new();

    private static CreateStockItemCommand CreateCommand(
        string? name = "Indomie Goreng",
        string? sku = "SKU-001",
        string? unit = "Pcs",
        string? costingMethod = "FIFO") =>
        new()
        {
            Name = name!,
            Sku = sku!,
            Unit = unit!,
            CostingMethod = costingMethod!,
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
        var result = _sut.Validate(CreateCommand(name: name));

        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .ContainSingle(e => e.PropertyName == nameof(CreateStockItemCommand.Name));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Validate_WithMissingSku_ReturnsError(string? sku)
    {
        var result = _sut.Validate(CreateCommand(sku: sku));

        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .ContainSingle(e => e.PropertyName == nameof(CreateStockItemCommand.Sku));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Validate_WithMissingUnit_ReturnsError(string? unit)
    {
        var result = _sut.Validate(CreateCommand(unit: unit));

        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .ContainSingle(e => e.PropertyName == nameof(CreateStockItemCommand.Unit));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    [InlineData("AVERAGE")]
    public void Validate_WithInvalidCostingMethod_ReturnsError(string? costingMethod)
    {
        var result = _sut.Validate(CreateCommand(costingMethod: costingMethod));

        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e => e.PropertyName == nameof(CreateStockItemCommand.CostingMethod));
    }
}
