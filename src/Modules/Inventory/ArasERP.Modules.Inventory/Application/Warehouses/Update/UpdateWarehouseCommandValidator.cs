using FluentValidation;

namespace ArasERP.Modules.Inventory.Application.Warehouses.Update;

public sealed class UpdateWarehouseCommandValidator : AbstractValidator<UpdateWarehouseCommand>
{
    public UpdateWarehouseCommandValidator()
    {
        RuleFor(x => x.WarehouseId)
            .NotEmpty()
            .Must(id => Guid.TryParse(id, out _))
            .WithMessage("'WarehouseId' must be a valid GUID.");

        RuleFor(x => x.Name).NotEmpty();

        RuleFor(x => x.AddressId).NotEmpty();

        RuleFor(x => x.PersonInCharge).NotNull();

        When(
            x => true,
            () =>
            {
                RuleFor(x => x.PersonInCharge!.Name).NotEmpty();
            }
        );
    }
}
