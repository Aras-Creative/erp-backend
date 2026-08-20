using ArasERP.Modules.Address.Application.Abstractions;
using ArasERP.Modules.Address.Application.Search;
using ArasERP.Modules.Address.Application.Sync;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace ArasERP.Tests.Unit.Application.Address;

public class SearchAddressesQueryHandlerTests
{
    private readonly IAddressRepository _repository;
    private readonly IKeywordSyncStateCache _syncStateCache;
    private readonly AddressKeywordFetchLock _fetchLock;
    private readonly IOptions<AddressSyncOptions> _options;
    private readonly SearchAddressesQueryHandler _sut;
    private readonly AddressSyncerService _syncService;

    public SearchAddressesQueryHandlerTests()
    {
        _repository = Substitute.For<IAddressRepository>();
        _syncStateCache = Substitute.For<IKeywordSyncStateCache>();
        _fetchLock = new AddressKeywordFetchLock();
        _options = Options.Create(new AddressSyncOptions
        {
            Provider = "mengantar",
            FreshnessTtlSeconds = 3600,
            FreshnessCheckTimeoutSeconds = 3,
            FallbackTimeoutSeconds = 15,
        });

        _syncService = new AddressSyncerService(
            Substitute.For<IAddressProvider>(),
            _repository,
            _syncStateCache,
            _options);

        _sut = new SearchAddressesQueryHandler(
            _repository,
            _syncService,
            _syncStateCache,
            _fetchLock,
            _options,
            Substitute.For<ILogger<SearchAddressesQueryHandler>>());
    }

    [Fact]
    public async Task Handle_EmptyLocalResults_TriggersFallbackAndRequeries()
    {
        var query = new SearchAddressesQuery { Keyword = "jakarta", Limit = 10 };

        _repository.SearchAsync("jakarta", 10, Arg.Any<CancellationToken>())
            .Returns(
                _ => CreateEmptyResults(),
                _ => CreateSingleResult());

        var result = await _sut.Handle(query, CancellationToken.None);

        result.Should().HaveCount(1);
        await _repository.Received(2).SearchAsync("jakarta", 10, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_LocalResultsExistAndTTLHit_ReturnsWithoutSync()
    {
        var query = new SearchAddressesQuery { Keyword = "bandung", Limit = 10 };

        _repository.SearchAsync("bandung", 10, Arg.Any<CancellationToken>())
            .Returns(CreateSingleResult());

        _syncStateCache.Get("mengantar", "bandung")
            .Returns(new KeywordSyncState("etag-123", DateTimeOffset.UtcNow.AddMinutes(-10)));

        var result = await _sut.Handle(query, CancellationToken.None);

        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task Handle_LockNotAcquired_ReturnsWithoutSync()
    {
        var query = new SearchAddressesQuery { Keyword = "medan", Limit = 10 };

        _repository.SearchAsync("medan", 10, Arg.Any<CancellationToken>())
            .Returns(CreateSingleResult());

        _syncStateCache.Get("mengantar", "medan")
            .Returns(new KeywordSyncState("etag-old", DateTimeOffset.UtcNow.AddHours(-2)));

        using var competingLock = await _fetchLock.TryAcquireAsync("medan", TimeSpan.FromSeconds(5));

        var result = await _sut.Handle(query, CancellationToken.None);

        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task Handle_NoExistingState_TriggersFreshnessCheck()
    {
        var query = new SearchAddressesQuery { Keyword = "yogyakarta", Limit = 10 };

        _repository.SearchAsync("yogyakarta", 10, Arg.Any<CancellationToken>())
            .Returns(CreateSingleResult());

        _syncStateCache.Get("mengantar", "yogyakarta")
            .Returns((KeywordSyncState?)null);

        var result = await _sut.Handle(query, CancellationToken.None);

        result.Should().HaveCount(1);
    }

    private static IReadOnlyList<ArasERP.Modules.Address.Domain.Address> CreateEmptyResults() => [];

    private static IReadOnlyList<ArasERP.Modules.Address.Domain.Address> CreateSingleResult()
    {
        return
        [
            ArasERP.Modules.Address.Domain.Address.Create(
                destinationCode: "DJJ20411",
                originCode: "DJJ20400",
                provinceName: "PAPUA",
                cityName: "MERAUKE",
                districtName: "MUTING",
                subDistrictName: "SEED AGUNG",
                zipCode: "99652"),
        ];
    }
}
