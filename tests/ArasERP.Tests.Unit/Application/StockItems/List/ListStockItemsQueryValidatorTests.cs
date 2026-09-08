using ArasERP.Modules.Inventory.Application.StockItems.List;
using FluentAssertions;

namespace ArasERP.Tests.Unit.Application.StockItems.List;

public class ListStockItemsQueryValidatorTests
{
    private readonly ListStockItemsQueryValidator _sut = new();

    private static ListStockItemsQuery CreateQuery() => new();

    [Fact]
    public void Validate_WithDefaults_ReturnsNoErrors()
    {
        var result = _sut.Validate(CreateQuery());

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_WithInvalidPage_ReturnsError(int page)
    {
        var query = CreateQuery();
        query.Page = page;

        var result = _sut.Validate(query);

        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .ContainSingle(e => e.PropertyName == nameof(ListStockItemsQuery.Page));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(101)]
    public void Validate_WithInvalidPageSize_ReturnsError(int pageSize)
    {
        var query = CreateQuery();
        query.PageSize = pageSize;

        var result = _sut.Validate(query);

        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .ContainSingle(e => e.PropertyName == nameof(ListStockItemsQuery.PageSize));
    }

    [Theory]
    [InlineData("")]
    [InlineData("Name")]
    [InlineData("Sku")]
    [InlineData("CreatedAt")]
    [InlineData("UpdatedAt")]
    public void Validate_WithAllowedOrderBy_ReturnsNoErrors(string? orderBy)
    {
        var query = CreateQuery();
        query.OrderBy = orderBy;

        var result = _sut.Validate(query);

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("Cost")]
    [InlineData("Unknown")]
    public void Validate_WithInvalidOrderBy_ReturnsError(string orderBy)
    {
        var query = CreateQuery();
        query.OrderBy = orderBy;

        var result = _sut.Validate(query);

        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .ContainSingle(e => e.PropertyName == nameof(ListStockItemsQuery.OrderBy));
    }

    [Fact]
    public void Validate_WithEmptyWarehouseId_ReturnsError()
    {
        var query = CreateQuery();
        query.WarehouseId = Guid.Empty;

        var result = _sut.Validate(query);

        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .ContainSingle(e => e.PropertyName == nameof(ListStockItemsQuery.WarehouseId));
    }

    [Fact]
    public void Validate_WithValidWarehouseId_ReturnsNoErrors()
    {
        var query = CreateQuery();
        query.WarehouseId = Guid.NewGuid();

        var result = _sut.Validate(query);

        result.IsValid.Should().BeTrue();
    }
}
