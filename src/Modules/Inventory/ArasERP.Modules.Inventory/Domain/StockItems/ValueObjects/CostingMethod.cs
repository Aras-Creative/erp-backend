using ArasERP.BuildingBlocks.Domain.Abstractions;

namespace ArasERP.Modules.Inventory.Domain.StockItems.ValueObjects;

public sealed class CostingMethod : ValueObject
{
    // Predefined instances
    public static readonly CostingMethod Fifo = new("FIFO");
    public static readonly CostingMethod Lifo = new("LIFO");
    public static readonly CostingMethod WeightedAverage = new("WEIGHTED_AVERAGE");

    private static readonly IReadOnlyDictionary<string, CostingMethod> _cache = 
        new Dictionary<string, CostingMethod>(StringComparer.OrdinalIgnoreCase)
    {
        ["FIFO"] = Fifo,
        ["LIFO"] = Lifo,
        ["WEIGHTED_AVERAGE"] = WeightedAverage
    };

    public string Value { get; }
    public string DisplayName => GetDisplayName();

    private CostingMethod(string value)
    {
        Value = value;
    }

    public static CostingMethod FromValue(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Costing method cannot be null or empty", nameof(value));

        if (_cache.TryGetValue(value, out var method))
            return method;

        throw new InvalidOperationException($"'{value}' is invalid CostingMethod. Valid values: {string.Join(", ", _cache.Keys)}");
    }

    public static bool IsValid(string value) => _cache.ContainsKey(value);

    public static IEnumerable<CostingMethod> List() => _cache.Values.Distinct();

    private string GetDisplayName() => Value switch
    {
        "FIFO" => "First In, First Out",
        "LIFO" => "Last In, First Out",
        "WEIGHTED_AVERAGE" => "Weighted Average",
        _ => Value
    };

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    // Implicit conversions
    public static implicit operator string(CostingMethod method) => method?.Value ?? string.Empty;
    public static explicit operator CostingMethod(string value) => FromValue(value);
}
