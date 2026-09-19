using ArasERP.BuildingBlocks.Domain.Abstractions;

namespace ArasERP.Modules.Inventory.Domain.StockMovements;

public sealed class Direction : ValueObject
{
    public static readonly Direction In = new("IN");
    public static readonly Direction Out = new("OUT");

    private static readonly IReadOnlyDictionary<string, Direction> _cache = new Dictionary<
        string,
        Direction
    >(StringComparer.OrdinalIgnoreCase)
    {
        ["IN"] = In,
        ["OUT"] = Out,
    };

    public string Value { get; }

    private Direction(string value)
    {
        Value = value;
    }

    public static Direction FromValue(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Direction cannot be null or empty", nameof(value));

        if (_cache.TryGetValue(value, out var direction))
            return direction;

        throw new InvalidOperationException(
            $"'{value}' is invalid Direction. Valid values: {string.Join(", ", _cache.Keys)}"
        );
    }

    public static bool IsValid(string? value) =>
        !string.IsNullOrWhiteSpace(value) && _cache.ContainsKey(value);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    public static implicit operator string(Direction direction) => direction?.Value ?? string.Empty;

    public static explicit operator Direction(string value) => FromValue(value);
}
