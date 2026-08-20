using ArasERP.BuildingBlocks.Application;
using ArasERP.Modules.Address.Domain;

namespace ArasERP.Modules.Address.Application.Search;

public sealed class SearchAddressesQuery : IQuery<IReadOnlyList<Domain.Address>>
{
    public string Keyword { get; init; } = null!;

    public int Limit { get; init; } = PaginationDefaults.MaxPageSize;
}
