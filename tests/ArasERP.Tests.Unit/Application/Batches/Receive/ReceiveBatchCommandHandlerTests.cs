using ArasERP.BuildingBlocks.Application;
using ArasERP.Modules.Inventory.Application.Abstractions;
using ArasERP.Modules.Inventory.Application.Batches.Receive;
using ArasERP.Modules.Inventory.Domain.Batches;
using ArasERP.Modules.Inventory.Domain.StockItems;
using ArasERP.Modules.Inventory.Domain.Warehouses;
using FluentAssertions;
using NSubstitute;

namespace ArasERP.Tests.Unit.Application.Batches.Receive;

public class ReceiveBatchCommandHandlerTests
{
    private readonly IBatchRepository _batchRepository = Substitute.For<IBatchRepository>();
    private readonly IStockItemRepository _stockItemRepository =
        Substitute.For<IStockItemRepository>();
    private readonly IWarehouseRepository _warehouseRepository =
        Substitute.For<IWarehouseRepository>();
    private readonly ReceiveBatchCommandValidator _validator = new();
    private readonly ReceiveBatchCommandHandler _sut;

    private readonly Guid _itemId = Guid.NewGuid();
    private readonly Guid _warehouseId = Guid.NewGuid();

    public ReceiveBatchCommandHandlerTests()
    {
        _sut = new ReceiveBatchCommandHandler(
            _batchRepository,
            _stockItemRepository,
            _warehouseRepository,
            _validator
        );

        _stockItemRepository
            .IsActiveAsync(Arg.Any<StockItemId>(), Arg.Any<CancellationToken>())
            .Returns(true);
        _warehouseRepository
            .IsActiveAsync(Arg.Any<WarehouseId>(), Arg.Any<CancellationToken>())
            .Returns(true);
    }

    private ReceiveBatchCommand CreateCommand(
        decimal receivedQty = 100,
        decimal unitCost = 10,
        string? receiptNumber = "RCV-001"
    ) =>
        new()
        {
            ItemId = _itemId,
            WarehouseId = _warehouseId,
            ReceivedQty = receivedQty,
            UnitCost = unitCost,
            ReceivedAt = DateTime.UtcNow,
            ReceiptNumber = receiptNumber!,
            RecordedBy = "budi",
        };

    [Fact]
    public async Task Handle_WithValidCommand_PersistsActiveBatch()
    {
        await _sut.Handle(CreateCommand(), CancellationToken.None);

        await _batchRepository
            .Received(1)
            .AddAsync(
                Arg.Is<Batch>(b =>
                    b.ItemId == new StockItemId(_itemId)
                    && b.WarehouseId == new WarehouseId(_warehouseId)
                    && b.ReceivedQty == 100
                    && b.RemainingQty == 100
                    && b.UnitCost == 10
                    && b.ReceiptNumber == "RCV-001"
                    && b.RecordedBy == "budi"
                    && b.Status == BatchStatus.Active
                ),
                Arg.Any<CancellationToken>()
            );
    }

    [Fact]
    public async Task Handle_WithDuplicateReceiptNumber_ThrowsAndDoesNotPersist()
    {
        _batchRepository
            .ExistsByReceiptNumberAsync("RCV-001", Arg.Any<CancellationToken>())
            .Returns(true);

        var act = async () => await _sut.Handle(CreateCommand(), CancellationToken.None);

        var exception = await act.Should().ThrowAsync<ValidationException>();
        exception.Which.Errors.Should().ContainSingle(e => e.Contains("already exists"));
        await _batchRepository.DidNotReceiveWithAnyArgs().AddAsync(default!, default);
    }

    [Fact]
    public async Task Handle_WithInactiveItem_ThrowsAndDoesNotPersist()
    {
        _stockItemRepository
            .IsActiveAsync(new StockItemId(_itemId), Arg.Any<CancellationToken>())
            .Returns(false);

        var act = async () => await _sut.Handle(CreateCommand(), CancellationToken.None);

        var exception = await act.Should().ThrowAsync<ValidationException>();
        exception.Which.Errors.Should().ContainSingle(e => e.Contains("item"));
        await _batchRepository.DidNotReceiveWithAnyArgs().AddAsync(default!, default);
    }

    [Fact]
    public async Task Handle_WithInactiveWarehouse_ThrowsAndDoesNotPersist()
    {
        _warehouseRepository
            .IsActiveAsync(new WarehouseId(_warehouseId), Arg.Any<CancellationToken>())
            .Returns(false);

        var act = async () => await _sut.Handle(CreateCommand(), CancellationToken.None);

        var exception = await act.Should().ThrowAsync<ValidationException>();
        exception
            .Which.Errors.Should()
            .ContainSingle(e => e.Contains("warehouse", StringComparison.OrdinalIgnoreCase));
        await _batchRepository.DidNotReceiveWithAnyArgs().AddAsync(default!, default);
    }

    [Fact]
    public async Task Handle_WithInvalidCommand_ThrowsAndDoesNotPersist()
    {
        var act = async () =>
            await _sut.Handle(CreateCommand(receivedQty: 0), CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
        await _batchRepository.DidNotReceiveWithAnyArgs().AddAsync(default!, default);
    }
}
