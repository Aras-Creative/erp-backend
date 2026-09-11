using ArasERP.BuildingBlocks.Application;
using ArasERP.Modules.Inventory.Application.Abstractions;
using ArasERP.Modules.Inventory.Application.StockMovements.List;
using FluentAssertions;
using FluentValidation;
using NSubstitute;

namespace ArasERP.Tests.Unit.Application.StockMovements.List;

public class ListStockMovementsQueryHandlerTests
{
    private readonly IStockMovementRepository _repository = Substitute.For<IStockMovementRepository>();
    private readonly ListStockMovementsQueryValidator _validator = new();
    private readonly ListStockMovementsQueryHandler _sut;

    public ListStockMovementsQueryHandlerTests()
    {
        _sut = new ListStockMovementsQueryHandler(_repository, _validator);
    }

    private static ListStockMovementsDto CreateDto() =>
        new()
        {
            Id = Guid.NewGuid(),
            ItemId = Guid.NewGuid(),
            ItemName = "Rice 5kg",
            WarehouseId = Guid.NewGuid(),
            BatchId = Guid.NewGuid(),
            Direction = "IN",
            Quantity = 100,
            SourceType = "PURCHASE",
            Note = "Stock received from purchase.",
            CreatedAt = DateTime.UtcNow,
            RecordedBy = Guid.NewGuid(),
            ReceivedBy = "budi",
        };

    [Fact]
    public async Task Handle_ReturnsPagedMovementsMappedToDtos()
    {
        var dto = CreateDto();
        var paged = new PagedList<ListStockMovementsDto>([dto], 1, 10, 1);
        _repository
            .ListAsync(Arg.Any<StockMovementListFilter>(), Arg.Any<CancellationToken>())
            .Returns(paged);

        var result = await _sut.Handle(new ListStockMovementsQuery(), CancellationToken.None);

        result.TotalCount.Should().Be(1);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(10);
        result.Items.Should().ContainSingle().Which.Should().BeEquivalentTo(dto);
    }

    [Fact]
    public async Task Handle_ForwardsFiltersToRepository()
    {
        var itemId = Guid.NewGuid();
        var batchId = Guid.NewGuid();
        var query = new ListStockMovementsQuery
        {
            ItemId = itemId,
            BatchId = batchId,
            Page = 2,
            PageSize = 25,
            OrderBy = "Quantity",
            Descending = true,
        };
        var paged = new PagedList<ListStockMovementsDto>([], 2, 25, 0);
        _repository
            .ListAsync(Arg.Any<StockMovementListFilter>(), Arg.Any<CancellationToken>())
            .Returns(paged);

        await _sut.Handle(query, CancellationToken.None);

        await _repository
            .Received(1)
            .ListAsync(
                Arg.Is<StockMovementListFilter>(f =>
                    f.ItemId == itemId
                    && f.BatchId == batchId
                    && f.Page == query.Page
                    && f.PageSize == query.PageSize
                    && f.OrderBy == query.OrderBy
                    && f.Descending == query.Descending
                ),
                Arg.Any<CancellationToken>()
            );
    }

    [Fact]
    public async Task Handle_WithNoMovements_ReturnsEmptyPage()
    {
        var paged = new PagedList<ListStockMovementsDto>([], 1, 10, 0);
        _repository
            .ListAsync(Arg.Any<StockMovementListFilter>(), Arg.Any<CancellationToken>())
            .Returns(paged);

        var result = await _sut.Handle(new ListStockMovementsQuery(), CancellationToken.None);

        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
        result.TotalPages.Should().Be(0);
    }

    [Fact]
    public async Task Handle_WithInvalidQuery_ThrowsValidationException()
    {
        var query = new ListStockMovementsQuery { Page = 0 };

        var act = async () => await _sut.Handle(query, CancellationToken.None);

        await act.Should().ThrowAsync<FluentValidation.ValidationException>();
    }
}