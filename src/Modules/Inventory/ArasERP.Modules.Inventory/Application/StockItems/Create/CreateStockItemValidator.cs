using ArasERP.Modules.Inventory.Domain.StockItems.ValueObjects;
using FluentValidation;

namespace ArasERP.Modules.Inventory.Application.StockItems.Create;

public sealed class CreateStockItemValidator : AbstractValidator<CreateStockItemCommand>
{
    public CreateStockItemValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.Sku).NotEmpty();
        RuleFor(x => x.Unit).NotEmpty();

        RuleFor(x => x.CostingMethod)
            .NotEmpty()
            .Must(TryParseCostingMethod)
            .WithMessage("'CostingMethod' must be a valid value (FIFO, LIFO, WEIGHTED_AVERAGE).");
    }

    private static bool TryParseCostingMethod(string value)
    {
        try
        {
            CostingMethod.FromValue(value);
            return true;
        }
        catch (Exception ex) when (ex is InvalidOperationException or ArgumentException)
        {
            return false;
        }
    }
}
