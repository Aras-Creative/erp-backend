using ArasERP.BuildingBlocks.Application;
using ArasERP.Modules.Address.Application.Abstractions;
using ArasERP.Modules.Address.Contracts.Addresses;
using FluentValidation;
using ValidationException = ArasERP.BuildingBlocks.Application.ValidationException;

namespace ArasERP.Modules.Address.Application.Addresses.Search;

public sealed class SearchAddressesQueryHandler(
    IAddressRepository addressRepository,
    IValidator<SearchAddressesQuery> validator
    )
        : IQueryHandler<SearchAddressesQuery, IReadOnlyList<AddressSearchResultItemDto>>
{
    private readonly IAddressRepository _addressRepository = addressRepository;

    private readonly IValidator<SearchAddressesQuery> _validator = validator;

    public async Task<IReadOnlyList<AddressSearchResultItemDto>> Handle(
        SearchAddressesQuery query,
        CancellationToken cancellationToken = default
    )
    {
        var validationResult = await _validator.ValidateAsync(query, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(
                validationResult.Errors.Select(e => e.ErrorMessage).ToList()
            );
        }

        return await _addressRepository.SearchAsync(
            query.Keyword.Trim(),
            query.Limit,
            cancellationToken
        );
    }
}
