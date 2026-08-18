namespace ArasERP.Integrations.Abstractions;

public interface IShippingProviderFactory
{
    IShippingProvider Get(string name);

    IReadOnlyList<IShippingProvider> GetAll();
}
