namespace ArasERP.Modules.Address.Contracts.Addresses;

public interface IAddressWriter
{
    Task UpsertAsync(
        IReadOnlyCollection<AddressSyncItem> items,
        CancellationToken cancellationToken = default
    );
}
