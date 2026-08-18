using ArasERP.Integrations.Abstractions;
using ArasERP.Integrations.Application;
using ArasERP.Integrations.Application.AddressSync;
using ArasERP.Integrations.Contracts.Addresses;
using ArasERP.Integrations.Options;
using ArasERP.Modules.Address.Application.Abstractions;
using ArasERP.Modules.Address.Contracts.Addresses;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace ArasERP.Tests.Unit.Application.Address;

public class AddressSearchFallbackServiceTests
{
    private readonly IAddressRepository _addressRepository = Substitute.For<IAddressRepository>();
    private readonly IKeywordSyncStateCache _syncStateCache = Substitute.For<IKeywordSyncStateCache>();
    private readonly IShippingProvider _provider = Substitute.For<IShippingProvider>();
    private readonly IShippingProviderFactory _providerFactory = Substitute.For<IShippingProviderFactory>();
    private readonly IAddressWriter _addressWriter = Substitute.For<IAddressWriter>();
    private readonly KeywordFetchLock _fetchLock = new();
    private readonly ILogger<AddressSearchFallbackService> _logger = Substitute.For<ILogger<AddressSearchFallbackService>>();
    private readonly AddressSearchFallbackService _sut;

    public AddressSearchFallbackServiceTests()
    {
        _provider.Name.Returns("mengantar");
        _providerFactory.Get("mengantar").Returns(_provider);

        var syncService = new AddressSyncService(_providerFactory, _addressWriter, _syncStateCache);
        var options = Options.Create(new AddressSyncOptions
        {
            Provider = "mengantar",
            FallbackTimeoutSeconds = 15,
            FreshnessCheckTimeoutSeconds = 3,
            FreshnessTtlSeconds = 3600,
        });

        _sut = new AddressSearchFallbackService(
            _addressRepository,
            syncService,
            _fetchLock,
            _syncStateCache,
            options,
            _logger
        );
    }

    private static AddressSearchResultItemDto CreateLocalResult(string keyword) =>
        new()
        {
            AddressId = Guid.NewGuid(),
            DestinationCode = "CBN20221",
            OriginCode = "CBN20200",
            ProvinceName = "JAWA BARAT",
            CityName = "KUNINGAN",
            DistrictName = "KRAMAT MULYA",
            SubDistrictName = "KALAPAGUNUNG",
            ZipCode = "45553",
        };

    private static AddressSearchResultDto CreateVendorResult() =>
        new(
            ProviderId: "ext-123",
            OriginCode: "CBN20200",
            DestinationCode: "CBN20221",
            ProvinceName: "JAWA BARAT",
            CityName: "KUNINGAN",
            DistrictName: "KRAMAT MULYA",
            SubDistrictName: "KALAPAGUNUNG",
            ZipCode: "45553"
        );

    [Fact]
    public async Task SearchAsync_NewKeyword_CallsFullSyncAndSetsLastCheckedAt()
    {
        _addressRepository.SearchAsync("kramat mulya", Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns([]);
        _addressRepository.SearchAsync("kramat mulya", Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns([CreateLocalResult("kramat mulya")]);
        _provider.SearchAddressesAsync("kramat mulya", null, Arg.Any<CancellationToken>())
            .Returns(new AddressSearchResult(false, "etag-1", [CreateVendorResult()]));

        var results = await _sut.SearchAsync("kramat mulya", 10);

        results.Should().NotBeEmpty();
        await _provider.Received(1).SearchAddressesAsync("kramat mulya", null, Arg.Any<CancellationToken>());
        _syncStateCache.Received(1).Set("mengantar", "kramat mulya", "etag-1", Arg.Any<DateTimeOffset>());
    }

    [Fact]
    public async Task SearchAsync_LocalResultsWithNullLastCheckedAt_CallsFreshnessCheck()
    {
        _addressRepository.SearchAsync("kramat mulya", Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns([CreateLocalResult("kramat mulya")]);
        _syncStateCache.Get("mengantar", "kramat mulya").Returns((KeywordSyncState?)null);
        _provider.SearchAddressesAsync("kramat mulya", null, Arg.Any<CancellationToken>())
            .Returns(new AddressSearchResult(false, "etag-1", [CreateVendorResult()]));

        var results = await _sut.SearchAsync("kramat mulya", 10);

        results.Should().NotBeEmpty();
        await _provider.Received(1).SearchAddressesAsync("kramat mulya", null, Arg.Any<CancellationToken>());
        _syncStateCache.Received(1).Set("mengantar", "kramat mulya", "etag-1", Arg.Any<DateTimeOffset>());
    }

    [Fact]
    public async Task SearchAsync_LocalResultsWithinTtl_SkipsVendorCall()
    {
        _addressRepository.SearchAsync("kramat mulya", Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns([CreateLocalResult("kramat mulya")]);
        _syncStateCache.Get("mengantar", "kramat mulya")
            .Returns(new KeywordSyncState("etag-1", DateTimeOffset.UtcNow.AddMinutes(-30)));

        var results = await _sut.SearchAsync("kramat mulya", 10);

        results.Should().NotBeEmpty();
        await _provider.DidNotReceive().SearchAddressesAsync(
            Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SearchAsync_LockNotAcquired_ReturnsLocalWithoutUpdatingLastCheckedAt()
    {
        _addressRepository.SearchAsync("kramat mulya", Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns([CreateLocalResult("kramat mulya")]);
        _syncStateCache.Get("mengantar", "kramat mulya").Returns((KeywordSyncState?)null);

        using var holder = await _fetchLock.TryAcquireAsync("kramat mulya", TimeSpan.FromSeconds(5));

        var results = await _sut.SearchAsync("kramat mulya", 10);

        results.Should().NotBeEmpty();
        _syncStateCache.DidNotReceive().Set(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<DateTimeOffset>());
    }

    [Fact]
    public async Task SearchAsync_Timeout_ReturnsLocalWithoutThrowing()
    {
        _addressRepository.SearchAsync("kramat mulya", Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns([CreateLocalResult("kramat mulya")]);
        _syncStateCache.Get("mengantar", "kramat mulya").Returns((KeywordSyncState?)null);
        _provider
            .When(p => p.SearchAddressesAsync("kramat mulya", null, Arg.Any<CancellationToken>()))
            .Do(_ => throw new OperationCanceledException());

        var results = await _sut.SearchAsync("kramat mulya", 10);

        results.Should().NotBeEmpty();
        results[0].DistrictName.Should().Be("KRAMAT MULYA");
    }
}
