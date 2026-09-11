using ArasERP.Modules.Inventory.Application.Batches.Create;
using FluentAssertions;

namespace ArasERP.Tests.Unit.Application.Batches.Create;

public class CreateBatchCommandValidatorTests
{
    private readonly CreateBatchCommandValidator _sut = new();

    private static CreateBatchCommand CreateCommand(
        Guid? itemId = null,
        Guid? warehouseId = null,
        decimal receivedQty = 100,
        decimal unitCost = 10,
        DateTime? receivedAt = null,
        string? sourceType = "PURCHASE"
    ) =>
        new()
        {
            ItemId = itemId ?? Guid.NewGuid(),
            WarehouseId = warehouseId ?? Guid.NewGuid(),
            ReceivedQty = receivedQty,
            UnitCost = unitCost,
            ReceivedAt = receivedAt ?? DateTime.UtcNow,
            SourceType = sourceType!,
            RecordedBy = Guid.NewGuid(),
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
            .ContainSingle(e => e.PropertyName == nameof(CreateBatchCommand.ReceivedQty));
    }

    [Fact]
    public void Validate_WithNegativeQuantity_ReturnsError()
    {
        var result = _sut.Validate(CreateCommand(receivedQty: -5));

        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .ContainSingle(e => e.PropertyName == nameof(CreateBatchCommand.ReceivedQty));
    }

    [Fact]
    public void Validate_WithNegativeUnitCost_ReturnsError()
    {
        var result = _sut.Validate(CreateCommand(unitCost: -1));

        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .ContainSingle(e => e.PropertyName == nameof(CreateBatchCommand.UnitCost));
    }

    [Fact]
    public void Validate_WithFutureReceivedAt_ReturnsError()
    {
        var result = _sut.Validate(CreateCommand(receivedAt: DateTime.UtcNow.AddHours(1)));

        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .ContainSingle(e => e.PropertyName == nameof(CreateBatchCommand.ReceivedAt));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Validate_WithMissingSourceType_ReturnsError(string? sourceType)
    {
        var result = _sut.Validate(CreateCommand(sourceType: sourceType));

        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .ContainSingle(e => e.PropertyName == nameof(CreateBatchCommand.SourceType));
    }

    [Fact]
    public void Validate_WithInvalidSourceType_ReturnsError()
    {
        var result = _sut.Validate(CreateCommand(sourceType: "INVENTORY_COUNT"));

        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .ContainSingle(e => e.PropertyName == nameof(CreateBatchCommand.SourceType));
    }

    [Theory]
    [InlineData("SALE")]
    [InlineData("ADJUSTMENT")]
    [InlineData("LOAN_OUT")]
    [InlineData("TRANSFER_OUT")]
    public void Validate_WithOutboundSourceType_ReturnsError(string sourceType)
    {
        var result = _sut.Validate(CreateCommand(sourceType: sourceType));

        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .ContainSingle(e => e.PropertyName == nameof(CreateBatchCommand.SourceType));
    }

    [Theory]
    [InlineData("PURCHASE")]
    [InlineData("CUSTOMER_RETURN")]
    [InlineData("LOAN_RETURN")]
    public void Validate_WithInboundSourceType_ReturnsNoErrors(string sourceType)
    {
        var result = _sut.Validate(CreateCommand(sourceType: sourceType));

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithEmptyItemId_ReturnsError()
    {
        var result = _sut.Validate(CreateCommand(itemId: Guid.Empty));

        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .ContainSingle(e => e.PropertyName == nameof(CreateBatchCommand.ItemId));
    }

    [Fact]
    public void Validate_WithEmptyWarehouseId_ReturnsError()
    {
        var result = _sut.Validate(CreateCommand(warehouseId: Guid.Empty));

        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .ContainSingle(e => e.PropertyName == nameof(CreateBatchCommand.WarehouseId));
    }
}