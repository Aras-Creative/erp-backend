using ArasERP.Modules.Address.Application.Abstractions;
using ArasERP.Modules.AddressClient;
using ArasERP.Modules.AddressClient.Dtos;

namespace ArasERP.Modules.Address.Application;

internal sealed class AddressClientImpl(IAddressRepository repository) : IAddressClient
{
    public async Task<AddressDto?> GetByIdAsync(
        Guid addressId,
        CancellationToken cancellationToken = default
    )
    {
        var address = await repository.GetByIdAsync(
            new Domain.AddressId(addressId),
            cancellationToken
        );

        if (address is null)
            return null;

        return new AddressDto
        {
            AddressId = address.Id.Value,
            DestinationCode = address.DestinationCode,
            OriginCode = address.OriginCode,
            ProvinceName = address.ProvinceName,
            CityName = address.CityName,
            DistrictName = address.DistrictName,
            SubDistrictName = address.SubDistrictName,
            ZipCode = address.ZipCode,
        };
    }
}
