using ArasERP.Modules.Inventory.Domain.StockMovements;
using FluentValidation;

namespace ArasERP.Modules.Inventory.Application.Batches.Create;

public sealed class CreateBatchCommandValidator : AbstractValidator<CreateBatchCommand>
{
    public CreateBatchCommandValidator()
    {
        RuleFor(x => x.ItemId).NotEmpty().WithMessage("'ItemId' must be a valid GUID.");

        RuleFor(x => x.WarehouseId).NotEmpty().WithMessage("'WarehouseId' must be a valid GUID.");

        RuleFor(x => x.ReceivedQty)
            .GreaterThan(0)
            .WithMessage("'ReceivedQty' must be greater than 0.");

        RuleFor(x => x.UnitCost)
            .GreaterThanOrEqualTo(0)
            .WithMessage("'UnitCost' must be greater than or equal to 0.");

        RuleFor(x => x.ReceivedAt)
            .NotEmpty()
            .WithMessage("'ReceivedAt' is required.")
            .Must(BePastOrNow)
            .WithMessage("'ReceivedAt' cannot be in the future.");

        RuleFor(x => x.SourceType)
            .NotEmpty()
            .WithMessage("'SourceType' is required.")
            .Must(SourceType.IsValidForBatchCreation)
            .WithMessage(
                "'SourceType' must be one of: PURCHASE, CUSTOMER_RETURN, LOAN_RETURN."
            )
            .When(
                x => !string.IsNullOrWhiteSpace(x.SourceType),
                ApplyConditionTo.CurrentValidator
            );

        RuleFor(x => x.ExternalReferenceNo).MaximumLength(100);

        RuleFor(x => x.Note).MaximumLength(500);

        RuleFor(x => x.ReceivedBy).MaximumLength(100);
    }

    private static bool BePastOrNow(CreateBatchCommand command, DateTime receivedAt) =>
        receivedAt.ToUniversalTime() <= DateTime.UtcNow;
}