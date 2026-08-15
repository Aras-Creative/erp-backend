using FluentValidation;

namespace ArasERP.Modules.Inventory.Application.Warehouses.Delete;

public sealed class DeleteWarehouseCommandValidator : AbstractValidator<DeleteWarehouseCommand>
{
    public DeleteWarehouseCommandValidator()
    {
        RuleFor(x => x.WarehouseId)
            .NotEmpty()
            .Must(id => Guid.TryParse(id, out _))
            .WithMessage("'WarehouseId' must be a valid GUID.");
    }
}
