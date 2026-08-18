using System.Collections.Concurrent;

namespace ArasERP.Integrations.Application;

public sealed class KeywordFetchLock
{
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new();

    public async Task<IDisposable?> TryAcquireAsync(
        string keyword,
        TimeSpan timeout,
        CancellationToken cancellationToken = default
    )
    {
        var semaphore = _locks.GetOrAdd(keyword, _ => new SemaphoreSlim(1, 1));

        var acquired = await semaphore.WaitAsync(timeout, cancellationToken);
        if (!acquired)
        {
            return null;
        }

        return new LockRelease(semaphore, keyword, this);
    }

    private void Release(string keyword, SemaphoreSlim semaphore)
    {
        semaphore.Release();
        _locks.TryRemove(keyword, out _);
    }

    private sealed class LockRelease : IDisposable
    {
        private readonly SemaphoreSlim _semaphore;
        private readonly string _keyword;
        private readonly KeywordFetchLock _owner;
        private bool _disposed;

        public LockRelease(SemaphoreSlim semaphore, string keyword, KeywordFetchLock owner)
        {
            _semaphore = semaphore;
            _keyword = keyword;
            _owner = owner;
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            _owner.Release(_keyword, _semaphore);
        }
    }
}
