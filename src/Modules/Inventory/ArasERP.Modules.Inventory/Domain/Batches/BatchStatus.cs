using ArasERP.BuildingBlocks.Domain.Abstractions;

namespace ArasERP.Modules.Inventory.Domain.Batches;

public sealed class BatchStatus : ValueObject
{
    public static readonly BatchStatus Active = new("ACTIVE");
    public static readonly BatchStatus Exhausted = new("EXHAUSTED");

    private static readonly IReadOnlyDictionary<string, BatchStatus> _cache = new Dictionary<
        string,
        BatchStatus
    >(StringComparer.OrdinalIgnoreCase)
    {
        ["ACTIVE"] = Active,
        ["EXHAUSTED"] = Exhausted,
    };

    public string Value { get; }

    private BatchStatus(string value)
    {
        Value = value;
    }

    public static BatchStatus FromValue(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Batch status cannot be null or empty", nameof(value));

        if (_cache.TryGetValue(value, out var status))
            return status;

        throw new InvalidOperationException(
            $"'{value}' is invalid BatchStatus. Valid values: {string.Join(", ", _cache.Keys)}"
        );
    }

    public static bool IsValid(string value) => _cache.ContainsKey(value);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    public static implicit operator string(BatchStatus status) => status?.Value ?? string.Empty;

    public static explicit operator BatchStatus(string value) => FromValue(value);
}
