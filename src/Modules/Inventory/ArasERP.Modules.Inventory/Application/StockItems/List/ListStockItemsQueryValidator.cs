using FluentValidation;

namespace ArasERP.Modules.Inventory.Application.StockItems.List;

public sealed class ListStockItemsQueryValidator : AbstractValidator<ListStockItemsQuery>
{
    private static readonly string[] AllowedOrderBy = { "Sku", "Name", "CreatedAt", "UpdatedAt" };

    public ListStockItemsQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThan(0);

        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);

        RuleFor(x => x.Search).MaximumLength(100);

        RuleFor(x => x.OrderBy)
            .Must(x => string.IsNullOrWhiteSpace(x) || AllowedOrderBy.Contains(x))
            .WithMessage("Invalid order by field.");

        RuleFor(x => x.WarehouseId)
            .Must(x => x is null || x != Guid.Empty)
            .WithMessage("Warehouse id must not be empty.");
    }
}
