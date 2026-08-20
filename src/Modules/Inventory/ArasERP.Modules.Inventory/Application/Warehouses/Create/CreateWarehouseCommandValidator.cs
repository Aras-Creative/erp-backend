using FluentValidation;

namespace ArasERP.Modules.Inventory.Application.Warehouses.Create;

public sealed class CreateWarehouseCommandValidator : AbstractValidator<CreateWarehouseCommand>
{
    public CreateWarehouseCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty();

        RuleFor(x => x.AddressId)
            .NotEmpty()
            .WithMessage("Address is required.");

        RuleFor(x => x.PersonInCharge).NotNull();

        When(
            x => x.PersonInCharge is not null,
            () =>
            {
                RuleFor(x => x.PersonInCharge!.Name).NotEmpty();
            }
        );
    }
}
