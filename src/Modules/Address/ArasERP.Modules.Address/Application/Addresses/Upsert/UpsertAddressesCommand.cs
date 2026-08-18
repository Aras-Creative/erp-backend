using ArasERP.BuildingBlocks.Application;
using ArasERP.Modules.Address.Contracts.Addresses;

namespace ArasERP.Modules.Address.Application.Addresses.Upsert;

public sealed class UpsertAddressesCommand : ICommand
{
    public required IReadOnlyList<AddressSyncItem> Items { get; init; }
}
