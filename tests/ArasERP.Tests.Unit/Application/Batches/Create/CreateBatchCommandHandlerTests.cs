using ArasERP.BuildingBlocks.Application;
using ArasERP.Modules.Inventory.Application.Abstractions;
using ArasERP.Modules.Inventory.Application.Batches.Create;
using ArasERP.Modules.Inventory.Domain.Batches;
using ArasERP.Modules.Inventory.Domain.StockItems;
using ArasERP.Modules.Inventory.Domain.StockLevels;
using ArasERP.Modules.Inventory.Domain.StockMovements;
using ArasERP.Modules.Inventory.Domain.Warehouses;
using FluentAssertions;
using NSubstitute;

namespace ArasERP.Tests.Unit.Application.Batches.Create;

public class CreateBatchCommandHandlerTests
{
    private readonly IBatchRepository _batchRepository = Substitute.For<IBatchRepository>();
    private readonly IStockItemRepository _stockItemRepository =
        Substitute.For<IStockItemRepository>();
    private readonly IWarehouseRepository _warehouseRepository =
        Substitute.For<IWarehouseRepository>();
    private readonly IStockLevelRepository _stockLevelRepository =
        Substitute.For<IStockLevelRepository>();
    private readonly IStockMovementRepository _stockMovementRepository =
        Substitute.For<IStockMovementRepository>();
    private readonly IInventoryUnitOfWork _unitOfWork = Substitute.For<IInventoryUnitOfWork>();
    private readonly CreateBatchCommandValidator _validator = new();
    private readonly CreateBatchCommandHandler _sut;

    private readonly Guid _itemId = Guid.NewGuid();
    private readonly Guid _warehouseId = Guid.NewGuid();

    public CreateBatchCommandHandlerTests()
    {
        _sut = new CreateBatchCommandHandler(
            _batchRepository,
            _stockItemRepository,
            _warehouseRepository,
            _stockLevelRepository,
            _stockMovementRepository,
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

    private readonly Guid _actorId = Guid.NewGuid();

    private CreateBatchCommand CreateCommand(
        decimal receivedQty = 100,
        decimal unitCost = 10,
        SourceTypeEnum sourceType = SourceTypeEnum.PURCHASE,
        string? externalReferenceNo = null,
        string? note = null
    ) =>
        new()
        {
            ItemId = _itemId,
            WarehouseId = _warehouseId,
            ReceivedQty = receivedQty,
            UnitCost = unitCost,
            ReceivedAt = DateTime.UtcNow,
            SourceType = sourceType,
            ExternalReferenceNo = externalReferenceNo,
            Note = note,
            ReceivedBy = "budi",
            RecordedBy = _actorId,
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
    public async Task Handle_WithPurchaseSource_RecordsInboundPurchaseMovement()
    {
        StockMovement? captured = null;
        _stockMovementRepository
            .When(x => x.AddAsync(Arg.Any<StockMovement>(), Arg.Any<CancellationToken>()))
            .Do(ci => captured = ci.Arg<StockMovement>());

        await _sut.Handle(
            CreateCommand(
                note: "Received from supplier",
                externalReferenceNo: "INV-SUPPLIER-00293"
            ),
            CancellationToken.None
        );

        await _stockMovementRepository
            .Received(1)
            .AddAsync(Arg.Any<StockMovement>(), Arg.Any<CancellationToken>());

        captured.Should().NotBeNull();
        captured!.ItemId.Should().Be(new StockItemId(_itemId));
        captured.WarehouseId.Should().Be(new WarehouseId(_warehouseId));
        captured.Direction.Should().Be(Direction.In);
        captured.Quantity.Should().Be(100);
        captured.UnitCost.Should().Be(10);
        captured.Currency.Should().Be("IDR");
        captured.Total.Should().Be(1000);
        captured.SourceType.Should().Be(SourceType.Purchase);
        captured.BatchId.Should().NotBeNull();
        captured.SourceReferenceId.Should().BeNull();
        captured.ExternalReferenceNo.Should().Be("INV-SUPPLIER-00293");
        captured.Note.Should().Be("Received from supplier");
        captured.RecordedBy.Should().Be(_actorId);
        captured.ReceivedBy.Should().Be("budi");
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
        await _stockMovementRepository.DidNotReceiveWithAnyArgs().AddAsync(default!, default);
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
        await _stockMovementRepository.DidNotReceiveWithAnyArgs().AddAsync(default!, default);
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
        await _stockMovementRepository.DidNotReceiveWithAnyArgs().AddAsync(default!, default);
    }
}
