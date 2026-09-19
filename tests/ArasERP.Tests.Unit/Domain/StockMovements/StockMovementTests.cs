using ArasERP.Modules.Inventory.Domain.Batches;
using ArasERP.Modules.Inventory.Domain.StockItems;
using ArasERP.Modules.Inventory.Domain.StockMovements;
using ArasERP.Modules.Inventory.Domain.Warehouses;
using FluentAssertions;

namespace ArasERP.Tests.Unit.Domain.StockMovements;

public class StockMovementTests
{
    private static readonly StockItemId ItemId = StockItemId.New();
    private static readonly WarehouseId WarehouseId = WarehouseId.New();

    private static StockMovement CreateMovement(
        decimal quantity = 50,
        decimal unitCost = 10,
        string currency = "IDR"
    ) =>
        StockMovement.Create(
            ItemId,
            WarehouseId,
            Direction.In,
            quantity,
            SourceType.Purchase,
            unitCost,
            currency
        );

    [Fact]
    public void Create_WithValidData_SetsSnapshotProperties()
    {
        var movement = CreateMovement();

        movement.ItemId.Should().Be(ItemId);
        movement.WarehouseId.Should().Be(WarehouseId);
        movement.Direction.Should().Be(Direction.In);
        movement.Quantity.Should().Be(50);
        movement.UnitCost.Should().Be(10);
        movement.Currency.Should().Be("IDR");
        movement.SourceType.Should().Be(SourceType.Purchase);
        movement.BatchId.Should().BeNull();
        movement.Id.Should().NotBeNull();
    }

    [Fact]
    public void Create_DefaultsCurrencyToIdr()
    {
        var movement = StockMovement.Create(
            ItemId,
            WarehouseId,
            Direction.In,
            50,
            SourceType.Purchase,
            10
        );

        movement.Currency.Should().Be("IDR");
    }

    [Fact]
    public void Create_NormalizesCurrencyToUppercase()
    {
        var movement = CreateMovement(currency: "idr");

        movement.Currency.Should().Be("IDR");
    }

    [Fact]
    public void Total_IsQuantityTimesUnitCost()
    {
        var movement = CreateMovement(quantity: 50, unitCost: 12.5m);

        movement.Total.Should().Be(625m);
    }

    [Fact]
    public void Create_WithBatchId_KeepsReference()
    {
        var batchId = BatchId.New();

        var movement = StockMovement.Create(
            ItemId,
            WarehouseId,
            Direction.In,
            50,
            SourceType.Purchase,
            10,
            batchId: batchId
        );

        movement.BatchId.Should().Be(batchId);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_WithNonPositiveQuantity_Throws(decimal quantity)
    {
        var act = () => CreateMovement(quantity: quantity);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Create_WithNegativeUnitCost_Throws()
    {
        var act = () => CreateMovement(unitCost: -0.01m);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Create_WithNullItem_Throws()
    {
        var act = () =>
            StockMovement.Create(null!, WarehouseId, Direction.In, 50, SourceType.Purchase, 10);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Create_WithNullWarehouse_Throws()
    {
        var act = () =>
            StockMovement.Create(ItemId, null!, Direction.In, 50, SourceType.Purchase, 10);

        act.Should().Throw<ArgumentNullException>();
    }
}
