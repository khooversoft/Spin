using System.Collections.Concurrent;
using Toolbox.Extensions;

namespace Toolbox.Tools;

public class AsyncKeyGuard
{
    private record KeyLock(string Key)
    {
        private int _refCount = 1;

        public SemaphoreSlim Semaphore { get; } = new SemaphoreSlim(1, 1);
        public int RefCount => _refCount;
        public KeyLock AddRef() => this.Action(_ => Interlocked.Increment(ref _refCount));
        public int ReleaseRef() => Interlocked.Decrement(ref _refCount);
    }

    private readonly ConcurrentDictionary<string, KeyLock> _keyLocks = new(StringComparer.OrdinalIgnoreCase);

    public bool IsLocked(string key)
    {
        key.NotEmpty();
        if (_keyLocks.TryGetValue(key, out KeyLock? keyLock))
        {
            return keyLock.Semaphore.CurrentCount == 0;
        }

        return false;
    }

    public bool IsRegistered(string key)
    {
        key.NotEmpty();
        return _keyLocks.ContainsKey(key);
    }

    public async Task<IDisposable> AcquireLock(string key, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        KeyLock keyLock = _keyLocks.AddOrUpdate(key, k => new KeyLock(k), (k, v) => v.AddRef());

        try
        {
            await keyLock.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        }
        catch
        {
            if (keyLock.ReleaseRef() <= 0 && _keyLocks.TryRemove(key, out KeyLock? removed))
            {
                removed.Semaphore.Dispose();
            }

            throw;
        }

        return new FinalizeScope(() =>
        {
            keyLock.Semaphore.Release();

            if (keyLock.ReleaseRef() <= 0 && _keyLocks.TryRemove(key, out KeyLock? removed))
            {
                removed.Semaphore.Dispose();
            }
        });
    }
}
