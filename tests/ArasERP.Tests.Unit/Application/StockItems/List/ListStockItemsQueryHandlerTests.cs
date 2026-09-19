using ArasERP.BuildingBlocks.Application;
using ArasERP.Modules.Inventory.Application.Abstractions;
using ArasERP.Modules.Inventory.Application.StockItems.List;
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

    [Fact]
    public async Task Handle_ReturnsPagedStockItemsMappedToDtos()
    {
        var dto = new ListStockItemsDto
        {
            Id = Guid.NewGuid(),
            Sku = "SKU-001",
            Name = "Indomie Goreng",
            Unit = "Pcs",
            IsActive = true,
            WarehouseId = Guid.NewGuid(),
            WarehouseName = "Gudang Utama",
            OnHandQty = 100,
            ReservedQty = 20,
            AvailableQty = 80,
        };
        var paged = new PagedList<ListStockItemsDto>([dto], 1, 10, 1);
        _repository
            .ListAsync(Arg.Any<StockItemListFilter>(), Arg.Any<CancellationToken>())
            .Returns(paged);

        var result = await _sut.Handle(new ListStockItemsQuery(), CancellationToken.None);

        result.TotalCount.Should().Be(1);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(10);
        result.Items.Should().ContainSingle().Which.Should().BeEquivalentTo(dto);
    }

    [Fact]
    public async Task Handle_ForwardsFilterToRepository()
    {
        var warehouseId = Guid.NewGuid();
        var query = new ListStockItemsQuery
        {
            Search = "Indomie",
            IsActive = true,
            Page = 2,
            PageSize = 25,
            OrderBy = "Name",
            Descending = true,
            WarehouseId = warehouseId,
        };
        var paged = new PagedList<ListStockItemsDto>([], 2, 25, 0);
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
                    && f.Descending == query.Descending
                    && f.WarehouseId == warehouseId
                ),
                Arg.Any<CancellationToken>()
            );
    }

    [Fact]
    public async Task Handle_WithNoStockItems_ReturnsEmptyPage()
    {
        var paged = new PagedList<ListStockItemsDto>([], 1, 10, 0);
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
