using ArasERP.Modules.Inventory.Application.StockMovements.List;
using FluentAssertions;

namespace ArasERP.Tests.Unit.Application.StockMovements.List;

public class ListStockMovementsQueryValidatorTests
{
    private readonly ListStockMovementsQueryValidator _sut = new();

    private static ListStockMovementsQuery CreateQuery(
        Guid? itemId = null,
        Guid? batchId = null,
        int page = 1,
        int pageSize = 20,
        string? orderBy = null
    ) =>
        new()
        {
            ItemId = itemId,
            BatchId = batchId,
            Page = page,
            PageSize = pageSize,
            OrderBy = orderBy,
        };

    [Fact]
    public void Validate_WithValidQuery_ReturnsNoErrors()
    {
        var result = _sut.Validate(CreateQuery());

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithItemAndBatchFilters_ReturnsNoErrors()
    {
        var result = _sut.Validate(
            CreateQuery(itemId: Guid.NewGuid(), batchId: Guid.NewGuid())
        );

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_WithInvalidPage_ReturnsError(int page)
    {
        var result = _sut.Validate(CreateQuery(page: page));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == nameof(ListStockMovementsQuery.Page));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(101)]
    public void Validate_WithInvalidPageSize_ReturnsError(int pageSize)
    {
        var result = _sut.Validate(CreateQuery(pageSize: pageSize));

        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .ContainSingle(e => e.PropertyName == nameof(ListStockMovementsQuery.PageSize));
    }

    [Fact]
    public void Validate_WithEmptyItemId_ReturnsError()
    {
        var result = _sut.Validate(CreateQuery(itemId: Guid.Empty));

        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .ContainSingle(e => e.PropertyName == nameof(ListStockMovementsQuery.ItemId));
    }

    [Fact]
    public void Validate_WithEmptyBatchId_ReturnsError()
    {
        var result = _sut.Validate(CreateQuery(batchId: Guid.Empty));

        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .ContainSingle(e => e.PropertyName == nameof(ListStockMovementsQuery.BatchId));
    }

    [Fact]
    public void Validate_WithInvalidOrderBy_ReturnsError()
    {
        var result = _sut.Validate(CreateQuery(orderBy: "Sku"));

        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .ContainSingle(e => e.PropertyName == nameof(ListStockMovementsQuery.OrderBy));
    }

    [Fact]
    public void Validate_WithValidOrderBy_ReturnsNoErrors()
    {
        var result = _sut.Validate(CreateQuery(orderBy: "Quantity"));

        result.IsValid.Should().BeTrue();
    }
}