using ArasERP.Modules.Inventory.Domain.StockItems;
using ArasERP.Modules.Inventory.Domain.StockLevels;
using ArasERP.Modules.Inventory.Domain.Warehouses;
using FluentAssertions;

namespace ArasERP.Tests.Unit.Domain.StockLevels;

public class StockLevelTests
{
    private static readonly StockItemId ItemId = StockItemId.New();
    private static readonly WarehouseId WarehouseId = WarehouseId.New();

    private static StockLevel CreateLevel() => StockLevel.Create(ItemId, WarehouseId);

    [Fact]
    public void Create_SetsProperties()
    {
        var level = CreateLevel();

        level.ItemId.Should().Be(ItemId);
        level.WarehouseId.Should().Be(WarehouseId);
        level.OnHandQty.Should().Be(0);
        level.ReservedQty.Should().Be(0);
        level.AvailableQty.Should().Be(0);
        level.HasStock.Should().BeFalse();
        level.Id.Should().NotBeNull();
    }

    [Fact]
    public void Create_WithNullItem_Throws()
    {
        var act = () => StockLevel.Create(null!, WarehouseId);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Create_WithNullWarehouse_Throws()
    {
        var act = () => StockLevel.Create(ItemId, null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Receive_WithNonPositiveQuantity_Throws(decimal qty)
    {
        var level = CreateLevel();

        var act = () => level.Receive(qty);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Receive_IncrementsOnHand()
    {
        var level = CreateLevel();

        level.Receive(50);

        level.OnHandQty.Should().Be(50);
        level.AvailableQty.Should().Be(50);
        level.HasStock.Should().BeTrue();
    }

    [Fact]
    public void Receive_AccumulatesAcrossCallsAndSetsUpdatedBy()
    {
        var level = CreateLevel();

        level.Receive(40, "budi");
        level.Receive(60);

        level.OnHandQty.Should().Be(100);
        level.UpdatedBy.Should().Be("budi");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Reserve_WithNonPositiveQuantity_Throws(decimal qty)
    {
        var level = CreateLevel();
        level.Receive(100);

        var act = () => level.Reserve(qty);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Reserve_IncrementsReservedAndReducesAvailable()
    {
        var level = CreateLevel();
        level.Receive(100);

        level.Reserve(30);

        level.ReservedQty.Should().Be(30);
        level.AvailableQty.Should().Be(70);
    }

    [Fact]
    public void Reserve_MoreThanAvailable_Throws()
    {
        var level = CreateLevel();
        level.Receive(10);

        var act = () => level.Reserve(10.01m);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Reserve_EqualToAvailable_DepletesAvailable()
    {
        var level = CreateLevel();
        level.Receive(10);

        level.Reserve(10);

        level.AvailableQty.Should().Be(0);
        level.HasStock.Should().BeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Release_WithNonPositiveQuantity_Throws(decimal qty)
    {
        var level = CreateLevel();
        level.Receive(100);
        level.Reserve(30);

        var act = () => level.Release(qty);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Release_DecrementsReserved()
    {
        var level = CreateLevel();
        level.Receive(100);
        level.Reserve(30);

        level.Release(10);

        level.ReservedQty.Should().Be(20);
        level.AvailableQty.Should().Be(80);
    }

    [Fact]
    public void Release_MoreThanReserved_Throws()
    {
        var level = CreateLevel();
        level.Receive(100);
        level.Reserve(30);

        var act = () => level.Release(30.01m);

        act.Should().Throw<InvalidOperationException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Issue_WithNonPositiveQuantity_Throws(decimal qty)
    {
        var level = CreateLevel();
        level.Receive(100);

        var act = () => level.Issue(qty);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Issue_MoreThanOnHand_Throws()
    {
        var level = CreateLevel();
        level.Receive(10);

        var act = () => level.Issue(10.01m);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Issue_WhenNoReservations_OnlyReducesOnHand()
    {
        var level = CreateLevel();
        level.Receive(100);

        level.Issue(30);

        level.OnHandQty.Should().Be(70);
        level.ReservedQty.Should().Be(0);
        level.AvailableQty.Should().Be(70);
    }

    [Fact]
    public void Issue_WhenFullyReserved_ReducesOnHandAndReserved()
    {
        var level = CreateLevel();
        level.Receive(100);
        level.Reserve(40);
        level.Reserve(60);

        level.Issue(60);

        level.OnHandQty.Should().Be(40);
        level.ReservedQty.Should().Be(40);
        level.AvailableQty.Should().Be(0);
    }

    [Fact]
    public void Issue_WhenPartiallyReserved_ReducesOnHandAndReserved()
    {
        var level = CreateLevel();
        level.Receive(100);
        level.Reserve(40);

        level.Issue(70);

        level.OnHandQty.Should().Be(30);
        level.ReservedQty.Should().Be(0);
        level.AvailableQty.Should().Be(30);
    }
}
