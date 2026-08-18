using ArasERP.Integrations.Contracts.Addresses;

namespace ArasERP.Integrations.Abstractions;

public interface IShippingProvider
{
    string Name { get; }

    Task<AddressSearchResult> SearchAddressesAsync(
        string keyword,
        string? etag = null,
        CancellationToken cancellationToken = default
    );
}
