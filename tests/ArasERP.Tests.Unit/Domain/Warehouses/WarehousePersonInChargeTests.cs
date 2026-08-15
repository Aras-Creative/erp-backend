using ArasERP.Modules.Inventory.Domain.Warehouses.ValueObjects;
using FluentAssertions;

namespace ArasERP.Tests.Unit.Domain.Warehouses;

public class WarehousePersonInChargeTests
{
    [Fact]
    public void Create_WithValidData_SetsProperties()
    {
        var pic = WarehousePersonInCharge.Create("Budi", "08123456789");

        pic.Name.Should().Be("Budi");
        pic.Phone.Should().Be("08123456789");
    }

    [Fact]
    public void Create_WithNullPhone_LeavesItNull()
    {
        var pic = WarehousePersonInCharge.Create("Budi");

        pic.Phone.Should().BeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithNullOrWhiteSpaceName_Throws(string? name)
    {
        var act = () => WarehousePersonInCharge.Create(name!);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void TwoPersonInCharges_WithSameValues_AreEqual()
    {
        var first = WarehousePersonInCharge.Create("Budi", "08123456789");
        var second = WarehousePersonInCharge.Create("Budi", "08123456789");

        first.Should().Be(second);
    }

    [Fact]
    public void TwoPersonInCharges_WithDifferentValues_AreNotEqual()
    {
        var first = WarehousePersonInCharge.Create("Budi", "08123456789");
        var second = WarehousePersonInCharge.Create("Budi", "081298765432");

        first.Should().NotBe(second);
    }
}
