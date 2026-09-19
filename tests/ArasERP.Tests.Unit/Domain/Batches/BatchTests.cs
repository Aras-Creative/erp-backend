using ArasERP.Modules.Inventory.Domain.Batches;
using ArasERP.Modules.Inventory.Domain.StockItems;
using ArasERP.Modules.Inventory.Domain.Warehouses;
using FluentAssertions;

namespace ArasERP.Tests.Unit.Domain.Batches;

public class BatchTests
{
    private static readonly StockItemId ItemId = StockItemId.New();
    private static readonly WarehouseId WarehouseId = WarehouseId.New();

    private static Batch CreateBatch(
        DateTime? receivedAt = null,
        decimal receivedQty = 100,
        decimal unitCost = 10
    ) => Batch.Create(ItemId, WarehouseId, receivedAt ?? DateTime.UtcNow, receivedQty, unitCost);

    [Fact]
    public void Create_WithValidData_SetsProperties()
    {
        var batch = CreateBatch();

        batch.ItemId.Should().Be(ItemId);
        batch.WarehouseId.Should().Be(WarehouseId);
        batch.ReceivedQty.Should().Be(100);
        batch.RemainingQty.Should().Be(100);
        batch.UnitCost.Should().Be(10);
        batch.Status.Should().Be(BatchStatus.Active);
        batch.Id.Should().NotBeNull();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_WithNonPositiveQuantity_Throws(decimal qty)
    {
        var act = () => CreateBatch(receivedQty: qty);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Create_WithNegativeUnitCost_Throws()
    {
        var act = () => CreateBatch(unitCost: -0.01m);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Create_WithZeroUnitCost_AllowsFreeItem()
    {
        var batch = CreateBatch(unitCost: 0);

        batch.UnitCost.Should().Be(0);
    }

    [Fact]
    public void Create_WithFutureReceivedAt_Throws()
    {
        var act = () => CreateBatch(receivedAt: DateTime.UtcNow.AddDays(1));

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Consume_DecrementsRemainingQuantity()
    {
        var batch = CreateBatch();

        batch.Consume(30);

        batch.RemainingQty.Should().Be(70);
        batch.Status.Should().Be(BatchStatus.Active);
    }

    [Fact]
    public void Consume_WhenReachingZero_MarksBatchAsExhausted()
    {
        var batch = CreateBatch();

        batch.Consume(100);

        batch.RemainingQty.Should().Be(0);
        batch.Status.Should().Be(BatchStatus.Exhausted);
    }

    [Fact]
    public void Consume_MoreThanRemaining_Throws()
    {
        var batch = CreateBatch();

        var act = () => batch.Consume(101);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Consume_WithNegativeQuantity_Throws()
    {
        var batch = CreateBatch();

        var act = () => batch.Consume(-1);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }
}
