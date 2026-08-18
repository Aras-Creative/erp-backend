namespace ArasERP.Integrations.Abstractions;

public interface IKeywordEtagCache
{
    string? Get(string provider, string keyword);

    void Set(string provider, string keyword, string etag);
}
