using ArasERP.BuildingBlocks.Application;
using ArasERP.Modules.Inventory.Application.Abstractions;
using ArasERP.Modules.Inventory.Application.StockItems.Create;
using ArasERP.Modules.Inventory.Domain.StockItems;
using ArasERP.Modules.Inventory.Domain.StockItems.ValueObjects;
using FluentAssertions;
using NSubstitute;

namespace ArasERP.Tests.Unit.Application.StockItems.Create;

public class CreateStockItemCommandHandlerTests
{
    private readonly IStockItemRepository _repository = Substitute.For<IStockItemRepository>();
    private readonly CreateStockItemValidator _validator = new();
    private readonly CreateStockItemCommandHandler _sut;

    public CreateStockItemCommandHandlerTests()
    {
        _sut = new CreateStockItemCommandHandler(_repository, _validator);
    }

    private static CreateStockItemCommand CreateCommand(
        string? name = "Indomie Goreng",
        string? sku = "SKU-001",
        string? unit = "Pcs",
        string? costingMethod = "FIFO") =>
        new()
        {
            Name = name!,
            Sku = sku!,
            Unit = unit!,
            CostingMethod = costingMethod!,
        };

    [Fact]
    public async Task Handle_WithValidCommand_PersistsStockItem()
    {
        var command = CreateCommand();

        await _sut.Handle(command, CancellationToken.None);

        await _repository
            .Received(1)
            .AddAsync(
                Arg.Is<StockItem>(s =>
                    s.Name == command.Name
                    && s.Sku == command.Sku
                    && s.Unit == command.Unit
                    && s.CostingMethod == CostingMethod.Fifo
                    && s.IsActive
                ),
                Arg.Any<CancellationToken>()
            );
    }

    [Fact]
    public async Task Handle_WithInvalidCommand_ThrowsValidationExceptionAndDoesNotPersist()
    {
        var command = CreateCommand(name: "");

        var act = async () => await _sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
        await _repository.DidNotReceiveWithAnyArgs().AddAsync(default!, default);
    }

    [Fact]
    public async Task Handle_WithInvalidCostingMethod_ThrowsValidationException()
    {
        var command = CreateCommand(costingMethod: "AVERAGE");

        var act = async () => await _sut.Handle(command, CancellationToken.None);

        var exception = await act.Should().ThrowAsync<ValidationException>();
        exception.Which.Errors.Should().ContainSingle(e => e.Contains("CostingMethod"));
    }

    [Fact]
    public async Task Handle_WithDuplicateSku_ThrowsValidationExceptionAndDoesNotPersist()
    {
        var command = CreateCommand();
        _repository
            .ExistsBySkuAsync(command.Sku, null, Arg.Any<CancellationToken>())
            .Returns(true);

        var act = async () => await _sut.Handle(command, CancellationToken.None);

        var exception = await act.Should().ThrowAsync<ValidationException>();
        exception.Which.Errors.Should().ContainSingle(e => e.Contains("already exists"));
        await _repository.DidNotReceiveWithAnyArgs().AddAsync(default!, default);
    }
}
