using ArasERP.Modules.Inventory.Application.Batches.Receive;
using FluentAssertions;

namespace ArasERP.Tests.Unit.Application.Batches.Receive;

public class ReceiveBatchCommandValidatorTests
{
    private readonly ReceiveBatchCommandValidator _sut = new();

    private static ReceiveBatchCommand CreateCommand(
        Guid? itemId = null,
        Guid? warehouseId = null,
        decimal receivedQty = 100,
        decimal unitCost = 10,
        DateTime? receivedAt = null,
        string? receiptNumber = "RCV-001"
    ) =>
        new()
        {
            ItemId = itemId ?? Guid.NewGuid(),
            WarehouseId = warehouseId ?? Guid.NewGuid(),
            ReceivedQty = receivedQty,
            UnitCost = unitCost,
            ReceivedAt = receivedAt ?? DateTime.UtcNow,
            ReceiptNumber = receiptNumber!,
        };

    [Fact]
    public void Validate_WithValidCommand_ReturnsNoErrors()
    {
        var result = _sut.Validate(CreateCommand());

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithZeroQuantity_ReturnsError()
    {
        var result = _sut.Validate(CreateCommand(receivedQty: 0));

        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .ContainSingle(e => e.PropertyName == nameof(ReceiveBatchCommand.ReceivedQty));
    }

    [Fact]
    public void Validate_WithNegativeQuantity_ReturnsError()
    {
        var result = _sut.Validate(CreateCommand(receivedQty: -5));

        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .ContainSingle(e => e.PropertyName == nameof(ReceiveBatchCommand.ReceivedQty));
    }

    [Fact]
    public void Validate_WithNegativeUnitCost_ReturnsError()
    {
        var result = _sut.Validate(CreateCommand(unitCost: -1));

        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .ContainSingle(e => e.PropertyName == nameof(ReceiveBatchCommand.UnitCost));
    }

    [Fact]
    public void Validate_WithFutureReceivedAt_ReturnsError()
    {
        var result = _sut.Validate(CreateCommand(receivedAt: DateTime.UtcNow.AddHours(1)));

        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .ContainSingle(e => e.PropertyName == nameof(ReceiveBatchCommand.ReceivedAt));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Validate_WithMissingReceiptNumber_ReturnsError(string? receiptNumber)
    {
        var result = _sut.Validate(CreateCommand(receiptNumber: receiptNumber));

        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .ContainSingle(e => e.PropertyName == nameof(ReceiveBatchCommand.ReceiptNumber));
    }

    [Fact]
    public void Validate_WithEmptyItemId_ReturnsError()
    {
        var result = _sut.Validate(CreateCommand(itemId: Guid.Empty));

        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .ContainSingle(e => e.PropertyName == nameof(ReceiveBatchCommand.ItemId));
    }

    [Fact]
    public void Validate_WithEmptyWarehouseId_ReturnsError()
    {
        var result = _sut.Validate(CreateCommand(warehouseId: Guid.Empty));

        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .ContainSingle(e => e.PropertyName == nameof(ReceiveBatchCommand.WarehouseId));
    }
}
