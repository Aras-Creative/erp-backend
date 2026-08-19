using ArasERP.BuildingBlocks.Application;
using ArasERP.Modules.Address.Contracts.Addresses;

namespace ArasERP.Modules.Address.Application.Addresses.Search;

public sealed class SearchAddressesQuery : IQuery<IReadOnlyList<AddressSearchResultItemDto>>
{
    public string Keyword { get; init; } = null!;

    public int Limit { get; init; } = PaginationDefaults.MaxPageSize;
}
