using ArasERP.BuildingBlocks.Application;
using ArasERP.Modules.Inventory.Application.Abstractions;
using ArasERP.Modules.Inventory.Application.StockItems.List;
using ArasERP.Modules.Inventory.Domain.StockItems;
using ArasERP.Modules.Inventory.Domain.StockItems.ValueObjects;
using FluentAssertions;
using FluentValidation;
using NSubstitute;

namespace ArasERP.Tests.Unit.Application.StockItems.List;

public class ListStockItemsQueryHandlerTests
{
    private readonly IStockItemRepository _repository = Substitute.For<IStockItemRepository>();
    private readonly ListStockItemsQueryValidator _validator = new();
    private readonly ListStockItemsQueryHandler _sut;

    public ListStockItemsQueryHandlerTests()
    {
        _sut = new ListStockItemsQueryHandler(_repository, _validator);
    }

    private static StockItem CreateItem(string name = "Indomie Goreng", string sku = "SKU-001") =>
        StockItem.Create(StockItemId.New(), name, sku, "Pcs", CostingMethod.Fifo);

    [Fact]
    public async Task Handle_ReturnsPagedStockItemsMappedToDtos()
    {
        var item = CreateItem();
        var paged = new PagedList<StockItem>([item], 1, 10, 1);
        _repository
            .ListAsync(Arg.Any<StockItemListFilter>(), Arg.Any<CancellationToken>())
            .Returns(paged);

        var result = await _sut.Handle(new ListStockItemsQuery(), CancellationToken.None);

        result.TotalCount.Should().Be(1);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(10);
        result.Items.Should().ContainSingle()
            .Which.Should().BeEquivalentTo(new ListStockItemsDto
            {
                Id = item.Id.Value,
                Sku = item.Sku,
                Name = item.Name,
                Unit = item.Unit,
                IsActive = item.IsActive,
            });
    }

    [Fact]
    public async Task Handle_ForwardsFilterToRepository()
    {
        var query = new ListStockItemsQuery
        {
            Search = "Indomie",
            IsActive = true,
            Page = 2,
            PageSize = 25,
            OrderBy = "Name",
            Descending = true,
        };
        var paged = new PagedList<StockItem>([], 2, 25, 0);
        _repository
            .ListAsync(Arg.Any<StockItemListFilter>(), Arg.Any<CancellationToken>())
            .Returns(paged);

        await _sut.Handle(query, CancellationToken.None);

        await _repository
            .Received(1)
            .ListAsync(
                Arg.Is<StockItemListFilter>(f =>
                    f.Search == query.Search
                    && f.IsActive == query.IsActive
                    && f.Page == query.Page
                    && f.PageSize == query.PageSize
                    && f.OrderBy == query.OrderBy
                    && f.Descending == query.Descending),
                Arg.Any<CancellationToken>()
            );
    }

    [Fact]
    public async Task Handle_WithNoStockItems_ReturnsEmptyPage()
    {
        var paged = new PagedList<StockItem>([], 1, 10, 0);
        _repository
            .ListAsync(Arg.Any<StockItemListFilter>(), Arg.Any<CancellationToken>())
            .Returns(paged);

        var result = await _sut.Handle(new ListStockItemsQuery(), CancellationToken.None);

        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
        result.TotalPages.Should().Be(0);
    }

    [Fact]
    public async Task Handle_WithInvalidQuery_ThrowsValidationException()
    {
        var query = new ListStockItemsQuery { Page = 0 };

        var act = async () => await _sut.Handle(query, CancellationToken.None);

        await act.Should().ThrowAsync<FluentValidation.ValidationException>();
    }
}
