using ArasERP.BuildingBlocks.Application;
using ArasERP.Modules.Address.Application.Abstractions;

namespace ArasERP.Modules.Address.Application.Addresses.Upsert;

public sealed class UpsertAddressesCommandHandler : ICommandHandler<UpsertAddressesCommand>
{
    private readonly IAddressRepository _addressRepository;

    public UpsertAddressesCommandHandler(IAddressRepository addressRepository)
    {
        _addressRepository = addressRepository;
    }

    public async Task Handle(
        UpsertAddressesCommand command,
        CancellationToken cancellationToken = default
    )
    {
        var items = command.Items;
        if (items.Count == 0)
        {
            return;
        }

        await _addressRepository.UpsertRangeAsync(items, cancellationToken);
    }
}
