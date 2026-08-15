using ArasERP.BuildingBlocks.Application;
using FluentAssertions;

namespace ArasERP.Tests.Unit.Shared;

public class PagedListTests
{
    [Fact]
    public void Constructor_ComputesTotalPagesAndNavigationFlags()
    {
        var list = new PagedList<int>(new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, 2, 10, 11);

        list.Page.Should().Be(2);
        list.PageSize.Should().Be(10);
        list.TotalCount.Should().Be(11);
        list.TotalPages.Should().Be(2);
        list.HasNextPage.Should().BeFalse();
        list.HasPreviousPage.Should().BeTrue();
    }

    [Fact]
    public void Constructor_WithEmptyItems_HasZeroTotalPages()
    {
        var list = new PagedList<int>([], 1, 10, 0);

        list.TotalPages.Should().Be(0);
        list.HasNextPage.Should().BeFalse();
        list.HasPreviousPage.Should().BeFalse();
    }

    [Fact]
    public void Constructor_WithPartialLastPage_SetsHasNextPage()
    {
        var list = new PagedList<int>(new[] { 6, 7, 8 }, 2, 5, 8);

        list.TotalPages.Should().Be(2);
        list.HasNextPage.Should().BeFalse();
        list.HasPreviousPage.Should().BeTrue();
    }

    [Fact]
    public void Map_TransformsItemsAndPreservesPaging()
    {
        var list = new PagedList<int>(new[] { 1, 2, 3 }, 1, 10, 3);

        var mapped = list.Map(x => x.ToString());

        mapped.Items.Should().BeEquivalentTo("1", "2", "3");
        mapped.Page.Should().Be(1);
        mapped.PageSize.Should().Be(10);
        mapped.TotalCount.Should().Be(3);
    }
}
