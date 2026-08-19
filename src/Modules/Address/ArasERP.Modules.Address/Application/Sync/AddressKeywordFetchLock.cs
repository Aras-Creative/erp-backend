using System.Collections.Concurrent;

namespace ArasERP.Modules.Address.Application.Sync;

public class AddressKeywordFetchLock
{
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new();

    public async Task<IDisposable?> TryAcquireAsync(
        string keyword,
        TimeSpan timeout,
        CancellationToken cancellationToken = default)
    {
        var semaphore = _locks.GetOrAdd(keyword, _ => new SemaphoreSlim(1, 1));

        var acquired = await semaphore.WaitAsync(timeout, cancellationToken);
        return !acquired ? null : new LockRelease(semaphore, keyword, this);
    }

    private void Release(string keyword, SemaphoreSlim semaphore)
    {
        semaphore.Release();
        _locks.TryRemove(keyword, out _);
    }

    private sealed class LockRelease(SemaphoreSlim semaphore, string keyword, AddressKeywordFetchLock owner)
        : IDisposable
    {
        private bool _disposed;

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            owner.Release(keyword, semaphore);
        }
    }
}
