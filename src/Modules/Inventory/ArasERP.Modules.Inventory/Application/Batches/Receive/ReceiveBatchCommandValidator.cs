using FluentValidation;

namespace ArasERP.Modules.Inventory.Application.Batches.Receive;

public sealed class ReceiveBatchCommandValidator : AbstractValidator<ReceiveBatchCommand>
{
    public ReceiveBatchCommandValidator()
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

        RuleFor(x => x.ReceiptNumber).NotEmpty().MaximumLength(50);

        RuleFor(x => x.RecordedBy).MaximumLength(100);
    }

    private static bool BePastOrNow(ReceiveBatchCommand command, DateTime receivedAt) =>
        receivedAt.ToUniversalTime() <= DateTime.UtcNow;
}
