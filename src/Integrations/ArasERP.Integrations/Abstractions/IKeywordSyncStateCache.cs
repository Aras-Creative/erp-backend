namespace ArasERP.Integrations.Abstractions;

public interface IKeywordSyncStateCache
{
    KeywordSyncState? Get(string provider, string keyword);

    void Set(string provider, string keyword, string? etag, DateTimeOffset lastCheckedAt);
}

public sealed record KeywordSyncState(string? ETag, DateTimeOffset LastCheckedAt);
