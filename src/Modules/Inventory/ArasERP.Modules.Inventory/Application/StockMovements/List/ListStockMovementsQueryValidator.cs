using FluentValidation;

namespace ArasERP.Modules.Inventory.Application.StockMovements.List;

public sealed class ListStockMovementsQueryValidator : AbstractValidator<ListStockMovementsQuery>
{
    private static readonly string[] AllowedOrderBy =
    {
        "CreatedAt",
        "Quantity",
        "Direction",
        "SourceType",
    };

    public ListStockMovementsQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThan(0);

        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);

        RuleFor(x => x.OrderBy)
            .Must(x => string.IsNullOrWhiteSpace(x) || AllowedOrderBy.Contains(x, StringComparer.OrdinalIgnoreCase))
            .WithMessage("Invalid order by field.");

        RuleFor(x => x.ItemId)
            .Must(x => x is null || x != Guid.Empty)
            .WithMessage("Item id must not be empty.");

        RuleFor(x => x.BatchId)
            .Must(x => x is null || x != Guid.Empty)
            .WithMessage("Batch id must not be empty.");
    }
}