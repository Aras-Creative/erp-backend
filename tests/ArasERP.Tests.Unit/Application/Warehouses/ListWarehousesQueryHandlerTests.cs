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
    public async Task Handle_ReturnsWarehousesFromRepository()
    {
        var expected = new[] { CreateItem("Gudang Utama"), CreateItem("Gudang Cabang") };
        _repository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(expected);

        var result = await _sut.Handle(new ListWarehousesQuery(), CancellationToken.None);

        result.Should().BeEquivalentTo(expected);
        await _repository.Received(1).GetAllAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithNoWarehouses_ReturnsEmptyList()
    {
        _repository.GetAllAsync(Arg.Any<CancellationToken>()).Returns([]);

        var result = await _sut.Handle(new ListWarehousesQuery(), CancellationToken.None);

        result.Should().BeEmpty();
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
