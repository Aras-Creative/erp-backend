using ArasERP.Modules.Inventory.Domain.Warehouses.ValueObjects;
using FluentAssertions;

namespace ArasERP.Tests.Unit.Domain.Warehouses;

public class WarehouseAddressTests
{
    private static readonly Guid AddressId = Guid.NewGuid();

    [Fact]
    public void Create_WithValidData_SetsProperties()
    {
        var address = WarehouseAddress.Create(
            AddressId,
            "Kebon Sirih",
            "Menteng",
            "Jakarta",
            "DKI Jakarta",
            "10110"
        );

        address.AddressId.Should().Be(AddressId);
        address.SubDistrictName.Should().Be("Kebon Sirih");
        address.DistrictName.Should().Be("Menteng");
        address.CityName.Should().Be("Jakarta");
        address.ProvinceName.Should().Be("DKI Jakarta");
        address.ZipCode.Should().Be("10110");
    }

    [Fact]
    public void Create_WithEmptyAddressId_Throws()
    {
        var act = () =>
            WarehouseAddress.Create(
                Guid.Empty,
                "Kebon Sirih",
                "Menteng",
                "Jakarta",
                "DKI Jakarta",
                "10110"
            );

        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData("", "District", "City", "State", "12345")]
    [InlineData(" ", "District", "City", "State", "12345")]
    [InlineData(null, "District", "City", "State", "12345")]
    [InlineData("SubDistrict", "", "City", "State", "12345")]
    [InlineData("SubDistrict", null, "City", "State", "12345")]
    [InlineData("SubDistrict", "District", "", "State", "12345")]
    [InlineData("SubDistrict", "District", null, "State", "12345")]
    [InlineData("SubDistrict", "District", "City", " ", "12345")]
    [InlineData("SubDistrict", "District", "City", null, "12345")]
    [InlineData("SubDistrict", "District", "City", "State", "")]
    [InlineData("SubDistrict", "District", "City", "State", null)]
    public void Create_WithNullOrWhiteSpaceRequiredComponent_Throws(
        string? subDistrictName,
        string? districtName,
        string? cityName,
        string? provinceName,
        string? zipCode
    )
    {
        var act = () =>
            WarehouseAddress.Create(
                AddressId,
                subDistrictName!,
                districtName!,
                cityName!,
                provinceName!,
                zipCode!
            );

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void TwoAddresses_WithSameValues_AreEqual()
    {
        var first = WarehouseAddress.Create(
            AddressId,
            "Kebon Sirih",
            "Menteng",
            "Jakarta",
            "DKI Jakarta",
            "10110"
        );
        var second = WarehouseAddress.Create(
            AddressId,
            "Kebon Sirih",
            "Menteng",
            "Jakarta",
            "DKI Jakarta",
            "10110"
        );

        first.Should().Be(second);
    }

    [Fact]
    public void TwoAddresses_WithDifferentValues_AreNotEqual()
    {
        var first = WarehouseAddress.Create(
            AddressId,
            "Kebon Sirih",
            "Menteng",
            "Jakarta",
            "DKI Jakarta",
            "10110"
        );
        var second = WarehouseAddress.Create(
            Guid.NewGuid(),
            "Batununggal",
            "Bandung Kota",
            "Bandung",
            "Jawa Barat",
            "40111"
        );

        first.Should().NotBe(second);
    }
}
