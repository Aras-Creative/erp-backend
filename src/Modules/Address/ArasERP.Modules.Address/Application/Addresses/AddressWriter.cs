using ArasERP.BuildingBlocks.Application;
using ArasERP.Modules.Address.Application.Addresses.Upsert;
using ArasERP.Modules.Address.Contracts.Addresses;

namespace ArasERP.Modules.Address.Application.Addresses;

public sealed class AddressWriter : IAddressWriter
{
    private readonly IMediator _mediator;

    public AddressWriter(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task UpsertAsync(
        IReadOnlyCollection<AddressSyncItem> items,
        CancellationToken cancellationToken = default
    )
    {
        await _mediator.SendAsync(
            new UpsertAddressesCommand { Items = items.ToList() },
            cancellationToken
        );
    }
}
