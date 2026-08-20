using ArasERP.Modules.Inventory.Application.Abstractions;
using ArasERP.Modules.Inventory.Application.Warehouses.GetById;
using FluentAssertions;
using NSubstitute;

namespace ArasERP.Tests.Unit.Application.Warehouses;

public class GetWarehouseByIdQueryHandlerTests
{
    private readonly IWarehouseRepository _repository = Substitute.For<IWarehouseRepository>();
    private readonly GetWarehouseByIdQueryHandler _sut;

    public GetWarehouseByIdQueryHandlerTests()
    {
        _sut = new GetWarehouseByIdQueryHandler(_repository);
    }

    [Fact]
    public async Task Handle_ReturnsWarehouseFromRepository()
    {
        var id = Guid.NewGuid();
        var expected = new WarehouseDetailDto
        {
            WarehouseId = id,
            Name = "Gudang Utama",
            PersonInCharge = new WarehouseDetailDto.PersonInChargeData
            {
                Name = "Budi",
                Phone = "08123456789",
            },
            Address = new WarehouseDetailDto.AddressData
            {
                SubDistrictName = "Kebon Sirih",
                DistrictName = "Menteng",
                CityName = "Jakarta",
                ProvinceName = "DKI Jakarta",
                ZipCode = "10110",
            },
        };
        _repository.GetDetailAsync(id, Arg.Any<CancellationToken>()).Returns(expected);

        var result = await _sut.Handle(new GetWarehouseByIdQuery(id), CancellationToken.None);

        result.Should().BeEquivalentTo(expected);
        await _repository.Received(1).GetDetailAsync(id, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenWarehouseNotFound_ReturnsNull()
    {
        _repository
            .GetDetailAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((WarehouseDetailDto?)null);

        var result = await _sut.Handle(
            new GetWarehouseByIdQuery(Guid.NewGuid()),
            CancellationToken.None
        );

        result.Should().BeNull();
    }
}
