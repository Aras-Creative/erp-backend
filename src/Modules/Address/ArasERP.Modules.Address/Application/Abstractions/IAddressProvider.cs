using ArasERP.Modules.Address.Domain;

namespace ArasERP.Modules.Address.Application.Abstractions;

public interface IAddressProvider
{
    Task<AddressProviderResult> SearchAsync(string keyword, string? etag, CancellationToken ct);
}

public sealed record AddressProviderResult(
    bool NotModified,
    string? Etag,
    IReadOnlyList<Domain.Address> Items
)
{
    public static AddressProviderResult Unchanged() => new(true, null, []);
}
