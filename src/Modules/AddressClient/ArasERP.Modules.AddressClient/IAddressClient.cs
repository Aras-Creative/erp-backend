using ArasERP.Modules.AddressClient.Dtos;

namespace ArasERP.Modules.AddressClient;

public interface IAddressClient
{
    Task<AddressDto?> GetByIdAsync(Guid addressId, CancellationToken cancellationToken = default);
}
