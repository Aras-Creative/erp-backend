using System.Collections.Concurrent;
using ArasERP.Modules.Address.Application.Abstractions;

namespace ArasERP.Modules.Address.Infrastructure;

public sealed class InMemoryKeywordSyncStateCache : IKeywordSyncStateCache
{
    private readonly ConcurrentDictionary<string, KeywordSyncState> _store = new();

    public KeywordSyncState? Get(string provider, string keyword)
    {
        return _store.TryGetValue(BuildKey(provider, keyword), out var state) ? state : null;
    }

    public void Set(string provider, string keyword, string? etag, DateTimeOffset lastCheckedAt)
    {
        _store[BuildKey(provider, keyword)] = new KeywordSyncState(etag, lastCheckedAt);
    }

    private static string BuildKey(string provider, string keyword) => $"{provider}:{keyword}";
}
