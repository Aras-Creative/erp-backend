using ArasERP.Modules.Inventory.Domain.Warehouses;
using ArasERP.Modules.Inventory.Domain.Warehouses.Events;
using ArasERP.Modules.Inventory.Domain.Warehouses.ValueObjects;
using FluentAssertions;

namespace ArasERP.Tests.Unit.Domain.Warehouses;

public class WarehouseTests
{
    private static WarehousePersonInCharge CreatePersonInCharge() =>
        WarehousePersonInCharge.Create("Budi", "08123456789");

    private static readonly Guid AddressId = Guid.NewGuid();

    private static WarehouseAddress CreateAddress() =>
        WarehouseAddress.Create(
            AddressId,
            "Kebon Sirih",
            "Menteng",
            "Jakarta",
            "DKI Jakarta",
            "10110"
        );

    [Fact]
    public void Create_WithValidData_SetsProperties()
    {
        var warehouse = Warehouse.Create(
            "Gudang Utama",
            CreatePersonInCharge(),
            CreateAddress(),
            "Sebelah pasar"
        );

        warehouse.Name.Should().Be("Gudang Utama");
        warehouse.PersonInCharge.Should().BeEquivalentTo(CreatePersonInCharge());
        warehouse.Address.Should().BeEquivalentTo(CreateAddress());
        warehouse.FullAddressText.Should().Be("Sebelah pasar");
        warehouse.Id.Should().NotBeNull();
        warehouse.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Create_WithNullFullAddressText_LeavesItNull()
    {
        var warehouse = Warehouse.Create("Gudang Utama", CreatePersonInCharge(), CreateAddress());

        warehouse.FullAddressText.Should().BeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithNullOrWhiteSpaceName_Throws(string? name)
    {
        var act = () => Warehouse.Create(name!, CreatePersonInCharge(), CreateAddress());

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithNullPersonInCharge_ThrowsArgumentNullException()
    {
        var act = () => Warehouse.Create("Gudang", null!, CreateAddress());

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Create_WithNullAddress_ThrowsArgumentNullException()
    {
        var act = () => Warehouse.Create("Gudang", CreatePersonInCharge(), null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Create_RaisesWarehouseCreatedDomainEvent()
    {
        var warehouse = Warehouse.Create("Gudang Utama", CreatePersonInCharge(), CreateAddress());

        warehouse
            .DomainEvents.Should()
            .ContainSingle()
            .Which.Should()
            .BeOfType<WarehouseCreatedDomainEvent>();
    }

    [Fact]
    public void Create_GeneratesUniqueIds()
    {
        var first = Warehouse.Create("Gudang A", CreatePersonInCharge(), CreateAddress());
        var second = Warehouse.Create("Gudang B", CreatePersonInCharge(), CreateAddress());

        first.Id.Should().NotBe(second.Id);
    }

    [Fact]
    public void Delete_MarksAsDeletedAndRaisesDeletedEvent()
    {
        var warehouse = Warehouse.Create("Gudang Utama", CreatePersonInCharge(), CreateAddress());

        warehouse.Delete();

        warehouse.IsDeleted.Should().BeTrue();
        warehouse.DeletedAtUtc.Should().NotBeNull();
        warehouse.DomainEvents.Should().ContainSingle(e => e is WarehouseDeletedDomainEvent);
    }

    [Fact]
    public void Delete_WhenAlreadyDeleted_IsIdempotent()
    {
        var warehouse = Warehouse.Create("Gudang Utama", CreatePersonInCharge(), CreateAddress());
        warehouse.Delete();
        var eventCount = warehouse.DomainEvents.Count;

        warehouse.Delete();

        warehouse.IsDeleted.Should().BeTrue();
        warehouse.DomainEvents.Count.Should().Be(eventCount);
    }

    [Fact]
    public void ToggleStatus_WhenActive_DeactivatesAndRaisesEvent()
    {
        var warehouse = Warehouse.Create("Gudang Utama", CreatePersonInCharge(), CreateAddress());

        warehouse.ToggleStatus();

        warehouse.IsActive.Should().BeFalse();
        warehouse
            .DomainEvents.OfType<WarehouseStatusChangedDomainEvent>()
            .Should()
            .ContainSingle()
            .Which.IsActive.Should()
            .BeFalse();
    }

    [Fact]
    public void ToggleStatus_WhenInactive_ActivatesAndRaisesEvent()
    {
        var warehouse = Warehouse.Create("Gudang Utama", CreatePersonInCharge(), CreateAddress());
        warehouse.ToggleStatus();

        warehouse.ToggleStatus();

        warehouse.IsActive.Should().BeTrue();
        warehouse
            .DomainEvents.OfType<WarehouseStatusChangedDomainEvent>()
            .Last()
            .IsActive.Should()
            .BeTrue();
    }
}
