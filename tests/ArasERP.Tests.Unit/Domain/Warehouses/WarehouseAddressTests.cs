using ArasERP.Modules.Inventory.Domain.Warehouses.ValueObjects;
using FluentAssertions;

namespace ArasERP.Tests.Unit.Domain.Warehouses;

public class WarehouseAddressTests
{
    [Fact]
    public void Create_WithValidData_SetsProperties()
    {
        var address = WarehouseAddress.Create(
            "Jl. Merdeka 1",
            "Jakarta",
            "DKI Jakarta",
            "10110",
            "Indonesia"
        );

        address.Street.Should().Be("Jl. Merdeka 1");
        address.City.Should().Be("Jakarta");
        address.State.Should().Be("DKI Jakarta");
        address.PostalCode.Should().Be("10110");
        address.Country.Should().Be("Indonesia");
    }

    [Fact]
    public void Create_WithNullCountry_LeavesItNull()
    {
        var address = WarehouseAddress.Create("Jl. Merdeka 1", "Jakarta", "DKI Jakarta", "10110");

        address.Country.Should().BeNull();
    }

    [Theory]
    [InlineData("", "City", "State", "12345")]
    [InlineData(" ", "City", "State", "12345")]
    [InlineData(null, "City", "State", "12345")]
    [InlineData("Street", "", "State", "12345")]
    [InlineData("Street", null, "State", "12345")]
    [InlineData("Street", "City", " ", "12345")]
    [InlineData("Street", "City", null, "12345")]
    [InlineData("Street", "City", "State", "")]
    [InlineData("Street", "City", "State", null)]
    public void Create_WithNullOrWhiteSpaceRequiredComponent_Throws(
        string? street,
        string? city,
        string? state,
        string? postalCode
    )
    {
        var act = () => WarehouseAddress.Create(street!, city!, state!, postalCode!);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void TwoAddresses_WithSameValues_AreEqual()
    {
        var first = WarehouseAddress.Create(
            "Jl. Merdeka 1",
            "Jakarta",
            "DKI Jakarta",
            "10110",
            "Indonesia"
        );
        var second = WarehouseAddress.Create(
            "Jl. Merdeka 1",
            "Jakarta",
            "DKI Jakarta",
            "10110",
            "Indonesia"
        );

        first.Should().Be(second);
    }

    [Fact]
    public void TwoAddresses_WithDifferentValues_AreNotEqual()
    {
        var first = WarehouseAddress.Create(
            "Jl. Merdeka 1",
            "Jakarta",
            "DKI Jakarta",
            "10110",
            "Indonesia"
        );
        var second = WarehouseAddress.Create(
            "Jl. Sudirman 2",
            "Jakarta",
            "DKI Jakarta",
            "10220",
            "Indonesia"
        );

        first.Should().NotBe(second);
    }
}
