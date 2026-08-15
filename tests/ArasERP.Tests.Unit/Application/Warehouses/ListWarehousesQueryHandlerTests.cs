using ArasERP.BuildingBlocks.Application;
using ArasERP.Modules.Inventory.Application.Abstractions;
using ArasERP.Modules.Inventory.Application.Warehouses.List;
using FluentAssertions;
using NSubstitute;

namespace ArasERP.Tests.Unit.Application.Warehouses;

public class ListWarehousesQueryHandlerTests
{
    private readonly IWarehouseRepository _repository = Substitute.For<IWarehouseRepository>();
    private readonly ListWarehousesQueryHandler _sut;

    public ListWarehousesQueryHandlerTests()
    {
        _sut = new ListWarehousesQueryHandler(_repository);
    }

    [Fact]
    public async Task Handle_ReturnsPagedWarehousesFromRepository()
    {
        var expected = new[] { CreateItem("Gudang Utama"), CreateItem("Gudang Cabang") };
        var paged = new PagedList<WarehouseListItemDto>(expected, 1, 10, 2);
        _repository.GetPagedAsync(1, 10, Arg.Any<CancellationToken>()).Returns(paged);

        var result = await _sut.Handle(new ListWarehousesQuery(), CancellationToken.None);

        result.Should().BeEquivalentTo(paged);
        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
        await _repository.Received(1).GetPagedAsync(1, 10, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ForwardsPaginationToRepository()
    {
        var expected = new PagedList<WarehouseListItemDto>([], 2, 25, 0);
        _repository.GetPagedAsync(2, 25, Arg.Any<CancellationToken>()).Returns(expected);

        var result = await _sut.Handle(new ListWarehousesQuery(2, 25), CancellationToken.None);

        result.Page.Should().Be(2);
        result.PageSize.Should().Be(25);
        await _repository.Received(1).GetPagedAsync(2, 25, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithNoWarehouses_ReturnsEmptyPage()
    {
        var expected = new PagedList<WarehouseListItemDto>([], 1, 10, 0);
        _repository.GetPagedAsync(1, 10, Arg.Any<CancellationToken>()).Returns(expected);

        var result = await _sut.Handle(new ListWarehousesQuery(), CancellationToken.None);

        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
        result.TotalPages.Should().Be(0);
    }

    private static WarehouseListItemDto CreateItem(string name) =>
        new()
        {
            WarehouseId = Guid.NewGuid(),
            Name = name,
            PersonInCharge = new WarehouseListItemDto.PersonInChargeData("Budi", "08123456789"),
            Address = new WarehouseListItemDto.AddressData(
                "Jl. Merdeka 1",
                "Jakarta",
                "DKI Jakarta",
                "10110",
                "Indonesia"
            ),
        };
}
