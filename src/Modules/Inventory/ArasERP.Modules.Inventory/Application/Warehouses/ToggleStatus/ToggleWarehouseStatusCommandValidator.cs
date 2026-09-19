using FluentValidation;

namespace ArasERP.Modules.Inventory.Application.Warehouses.ToggleStatus;

public sealed class ToggleWarehouseStatusCommandValidator
    : AbstractValidator<ToggleWarehouseStatusCommand>
{
    public ToggleWarehouseStatusCommandValidator()
    {
        RuleFor(x => x.WarehouseId)
            .NotEmpty()
            .Must(id => Guid.TryParse(id, out _))
            .WithMessage("'WarehouseId' must be a valid GUID.");
    }
}
