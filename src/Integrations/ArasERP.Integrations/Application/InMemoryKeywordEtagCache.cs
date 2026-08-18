using System.Collections.Concurrent;
using ArasERP.Integrations.Abstractions;

namespace ArasERP.Integrations.Application;

public sealed class InMemoryKeywordEtagCache : IKeywordEtagCache
{
    private readonly ConcurrentDictionary<string, string> _store = new();

    public string? Get(string provider, string keyword)
    {
        return _store.TryGetValue(BuildKey(provider, keyword), out var etag) ? etag : null;
    }

    public void Set(string provider, string keyword, string etag)
    {
        _store[BuildKey(provider, keyword)] = etag;
    }

    private static string BuildKey(string provider, string keyword) => $"{provider}:{keyword}";
}
