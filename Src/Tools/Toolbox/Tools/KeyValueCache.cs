using System.Collections.Concurrent;
using Toolbox.Types;

namespace Toolbox.Tools;

public class KeyValueCache<T>
{
    private readonly ConcurrentDictionary<string, CacheObject<T>> _cache = new(StringComparer.OrdinalIgnoreCase);
    private readonly ReaderWriterLockSlim _gate = new();
    private readonly TimeSpan _lifeTime;

    public KeyValueCache(TimeSpan lifeTime) => _lifeTime = lifeTime;

    public int Count
    {
        get
        {
            _gate.EnterReadLock();
            try
            {
                return _cache.Count;
            }
            finally { _gate.ExitReadLock(); }
        }
    }

    public void ClearAll()
    {
        _gate.EnterWriteLock();
        try
        {
            _cache.Clear();
        }
        finally { _gate.ExitWriteLock(); }
    }

    public Option<T> TryGetValue(string key)
    {
        key.NotEmpty();

        _gate.EnterReadLock();
        try
        {
            if (!_cache.TryGetValue(key, out CacheObject<T>? cacheObject)) return StatusCode.NotFound;

            bool hasValue = cacheObject.TryPeekValue(out T? staleValue);
            if (cacheObject.TryGetValue(out T? validLease)) return validLease;

            _cache.TryRemove(key, out _);
            return hasValue ? new Option<T>(true, staleValue!, StatusCode.Conflict) : StatusCode.NotFound;
        }
        finally { _gate.ExitReadLock(); }
    }

    public void AddOrUpdate(string key, T value)
    {
        key.NotEmpty();

        _gate.EnterWriteLock();
        try
        {
            _cache.AddOrUpdate(
                key,
                new CacheObject<T>(_lifeTime).Set(value),
                (_, current) => current.Set(value)
            );
        }
        finally { _gate.ExitWriteLock(); }
    }

    public void Clear(string path)
    {
        path.NotEmpty();

        _gate.EnterWriteLock();
        try
        {
            _cache.TryRemove(path, out _);
        }
        finally { _gate.ExitWriteLock(); }
    }
}
