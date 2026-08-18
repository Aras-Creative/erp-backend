namespace ArasERP.Modules.Address.Contracts.Addresses;

public interface IAddressSearchFallbackService
{
    Task<IReadOnlyList<AddressSearchResultItemDto>> SearchAsync(
        string keyword,
        int limit,
        CancellationToken cancellationToken = default
    );
}
