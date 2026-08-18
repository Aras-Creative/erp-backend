using ArasERP.BuildingBlocks.Application;
using ArasERP.Integrations.Abstractions;

namespace ArasERP.Integrations.Application;

public sealed class ShippingProviderFactory : IShippingProviderFactory
{
    private readonly IReadOnlyList<IShippingProvider> _providers;

    public ShippingProviderFactory(IEnumerable<IShippingProvider> providers)
    {
        _providers = providers.ToList();
    }

    public IShippingProvider Get(string name)
    {
        return _providers.FirstOrDefault(p =>
                p.Name.Equals(name, StringComparison.OrdinalIgnoreCase)
            )
            ?? throw new ValidationException($"Shipping provider '{name}' is not registered.");
    }

    public IReadOnlyList<IShippingProvider> GetAll() => _providers;
}
