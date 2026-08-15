using FluentValidation;

namespace ArasERP.Modules.Inventory.Application.Warehouses.Create;

public sealed class CreateWarehouseCommandValidator : AbstractValidator<CreateWarehouseCommand>
{
    public CreateWarehouseCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty();

        RuleFor(x => x.Address).NotNull();

        When(
            x => x.Address is not null,
            () =>
            {
                RuleFor(x => x.Address!.Street).NotEmpty();

                RuleFor(x => x.Address!.City).NotEmpty();

                RuleFor(x => x.Address!.State).NotEmpty();

                RuleFor(x => x.Address!.PostalCode).NotEmpty();
            }
        );

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
