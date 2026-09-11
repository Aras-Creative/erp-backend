using ArasERP.BuildingBlocks.Domain.Abstractions;

namespace ArasERP.Modules.Inventory.Domain.StockMovements;

public sealed class SourceType : ValueObject
{
    public static readonly SourceType Purchase = new("PURCHASE");
    public static readonly SourceType Sale = new("SALE");
    public static readonly SourceType CustomerReturn = new("CUSTOMER_RETURN");
    public static readonly SourceType Adjustment = new("ADJUSTMENT");
    public static readonly SourceType LoanOut = new("LOAN_OUT");
    public static readonly SourceType LoanReturn = new("LOAN_RETURN");
    public static readonly SourceType TransferOut = new("TRANSFER_OUT");
    public static readonly SourceType TransferIn = new("TRANSFER_IN");

    private static readonly IReadOnlyDictionary<string, SourceType> _cache = new Dictionary<
        string,
        SourceType
    >(StringComparer.OrdinalIgnoreCase)
    {
        ["PURCHASE"] = Purchase,
        ["SALE"] = Sale,
        ["CUSTOMER_RETURN"] = CustomerReturn,
        ["ADJUSTMENT"] = Adjustment,
        ["LOAN_OUT"] = LoanOut,
        ["LOAN_RETURN"] = LoanReturn,
        ["TRANSFER_OUT"] = TransferOut,
        ["TRANSFER_IN"] = TransferIn,
    };

    public string Value { get; }

    public Direction? DefaultDirection =>
        this switch
        {
            _ when this == Purchase => Direction.In,
            _ when this == CustomerReturn => Direction.In,
            _ when this == LoanReturn => Direction.In,
            _ when this == TransferIn => Direction.In,
            _ when this == Sale => Direction.Out,
            _ when this == LoanOut => Direction.Out,
            _ when this == TransferOut => Direction.Out,
            _ => null,
        };

    private SourceType(string value)
    {
        Value = value;
    }

    public static SourceType FromValue(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Source type cannot be null or empty", nameof(value));

        if (_cache.TryGetValue(value, out var sourceType))
            return sourceType;

        throw new InvalidOperationException(
            $"'{value}' is invalid SourceType. Valid values: {string.Join(", ", _cache.Keys)}"
        );
    }

    public static bool IsValid(string? value) =>
        !string.IsNullOrWhiteSpace(value) && _cache.ContainsKey(value);

    public static IEnumerable<SourceType> List() => _cache.Values.Distinct();

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    public static implicit operator string(SourceType sourceType) => sourceType?.Value ?? string.Empty;

    public static explicit operator SourceType(string value) => FromValue(value);
}