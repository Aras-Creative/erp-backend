using ArasERP.BuildingBlocks.Application;
using ArasERP.Modules.Inventory.Application.Abstractions;
using ArasERP.Modules.Inventory.Application.Batches.Receive;
using ArasERP.Modules.Inventory.Domain.Batches;
using ArasERP.Modules.Inventory.Domain.StockItems;
using ArasERP.Modules.Inventory.Domain.StockLevels;
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
    private readonly IStockLevelRepository _stockLevelRepository =
        Substitute.For<IStockLevelRepository>();
    private readonly IInventoryUnitOfWork _unitOfWork = Substitute.For<IInventoryUnitOfWork>();
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
            _stockLevelRepository,
            _unitOfWork,
            _validator
        );

        _stockItemRepository
            .IsActiveAsync(Arg.Any<StockItemId>(), Arg.Any<CancellationToken>())
            .Returns(true);
        _warehouseRepository
            .IsActiveAsync(Arg.Any<WarehouseId>(), Arg.Any<CancellationToken>())
            .Returns(true);
        _unitOfWork
            .ExecuteInTransactionAsync(Arg.Any<Func<Task>>(), Arg.Any<CancellationToken>())
            .Returns(ci => ci.Arg<Func<Task>>()());
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
    public async Task Handle_WithValidCommand_PersistsBatchInTransaction()
    {
        await _sut.Handle(CreateCommand(), CancellationToken.None);

        await _unitOfWork
            .Received(1)
            .ExecuteInTransactionAsync(Arg.Any<Func<Task>>(), Arg.Any<CancellationToken>());
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
    public async Task Handle_WithValidCommand_CreatesStockLevelWithReceivedQty()
    {
        await _sut.Handle(CreateCommand(), CancellationToken.None);

        await _stockLevelRepository
            .Received(1)
            .AddAsync(
                Arg.Is<StockLevel>(l =>
                    l.ItemId == new StockItemId(_itemId)
                    && l.WarehouseId == new WarehouseId(_warehouseId)
                    && l.OnHandQty == 100
                    && l.ReservedQty == 0
                    && l.AvailableQty == 100
                ),
                Arg.Any<CancellationToken>()
            );
        await _stockLevelRepository.DidNotReceiveWithAnyArgs().UpdateAsync(default!, default);
    }

    [Fact]
    public async Task Handle_WhenStockLevelExists_UpdatesExistingStockLevel()
    {
        var existing = StockLevel.Create(new StockItemId(_itemId), new WarehouseId(_warehouseId));
        existing.Receive(50);

        _stockLevelRepository
            .GetByKeyAsync(
                new StockItemId(_itemId),
                new WarehouseId(_warehouseId),
                Arg.Any<CancellationToken>()
            )
            .Returns(existing);

        await _sut.Handle(CreateCommand(), CancellationToken.None);

        await _stockLevelRepository.DidNotReceiveWithAnyArgs().AddAsync(default!, default);
        await _stockLevelRepository
            .Received(1)
            .UpdateAsync(
                Arg.Is<StockLevel>(l => l.OnHandQty == 150 && l.AvailableQty == 150),
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
        await _stockLevelRepository.DidNotReceiveWithAnyArgs().AddAsync(default!, default);
        await _stockLevelRepository.DidNotReceiveWithAnyArgs().UpdateAsync(default!, default);
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
        await _stockLevelRepository.DidNotReceiveWithAnyArgs().AddAsync(default!, default);
        await _stockLevelRepository.DidNotReceiveWithAnyArgs().UpdateAsync(default!, default);
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
        await _stockLevelRepository.DidNotReceiveWithAnyArgs().AddAsync(default!, default);
        await _stockLevelRepository.DidNotReceiveWithAnyArgs().UpdateAsync(default!, default);
    }

    [Fact]
    public async Task Handle_WithInvalidCommand_ThrowsAndDoesNotPersist()
    {
        var act = async () =>
            await _sut.Handle(CreateCommand(receivedQty: 0), CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
        await _batchRepository.DidNotReceiveWithAnyArgs().AddAsync(default!, default);
        await _stockLevelRepository.DidNotReceiveWithAnyArgs().AddAsync(default!, default);
        await _stockLevelRepository.DidNotReceiveWithAnyArgs().UpdateAsync(default!, default);
    }
}
