using ArasERP.BuildingBlocks.Application;
using FluentValidation;

namespace ArasERP.Modules.Address.Application.Search;

public sealed class SearchAddressesQueryValidator : AbstractValidator<SearchAddressesQuery>
{
    public SearchAddressesQueryValidator()
    {
        RuleFor(x => x.Keyword)
            .NotEmpty()
            .WithMessage("A keyword is required to search addresses.")
            .Must(k => k.Trim().Length >= 3)
            .WithMessage("Keyword must be at least 3 characters.");

        RuleFor(x => x.Limit)
            .InclusiveBetween(1, PaginationDefaults.MaxPageSize)
            .WithMessage($"Limit must be between 1 and {PaginationDefaults.MaxPageSize}.");
    }
}
