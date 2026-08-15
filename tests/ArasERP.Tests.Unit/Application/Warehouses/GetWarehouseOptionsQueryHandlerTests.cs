using ArasERP.Modules.Inventory.Application.Abstractions;
using ArasERP.Modules.Inventory.Application.Warehouses.GetOptions;
using FluentAssertions;
using NSubstitute;

namespace ArasERP.Tests.Unit.Application.Warehouses;

public class GetWarehouseOptionsQueryHandlerTests
{
    private readonly IWarehouseRepository _repository = Substitute.For<IWarehouseRepository>();
    private readonly GetWarehouseOptionsQueryHandler _sut;

    public GetWarehouseOptionsQueryHandlerTests()
    {
        _sut = new GetWarehouseOptionsQueryHandler(_repository);
    }

    [Fact]
    public async Task Handle_ReturnsOptionsFromRepository()
    {
        var expected = new[]
        {
            new WarehouseOptionDto(Guid.NewGuid(), "Gudang Utama"),
            new WarehouseOptionDto(Guid.NewGuid(), "Gudang Cabang"),
        };
        _repository.GetOptionsAsync(Arg.Any<CancellationToken>()).Returns(expected);

        var result = await _sut.Handle(new GetWarehouseOptionsQuery(), CancellationToken.None);

        result.Should().BeEquivalentTo(expected);
        await _repository.Received(1).GetOptionsAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithNoWarehouses_ReturnsEmptyList()
    {
        _repository.GetOptionsAsync(Arg.Any<CancellationToken>()).Returns([]);

        var result = await _sut.Handle(new GetWarehouseOptionsQuery(), CancellationToken.None);

        result.Should().BeEmpty();
    }
}
