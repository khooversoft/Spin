using System.Collections.Concurrent;
using Toolbox.Types;

namespace Toolbox.Tools;

public class KeyValueCache<T>
{
    private readonly ConcurrentDictionary<string, CacheObject<T>> _cache = new(StringComparer.OrdinalIgnoreCase);
    private readonly object _lock = new object();
    private readonly TimeSpan _lifeTime;

    public KeyValueCache(TimeSpan lifeTime) => _lifeTime = lifeTime;

    public int Count
    {
        get
        {
            lock (_lock)
            {
                return _cache.Count;
            }
        }
    }

    public void ClearAll()
    {
        lock (_lock)
        {
            _cache.Clear();
        }
    }

    public Option<T> TryGetValue(string key)
    {
        key.NotEmpty();

        lock (_lock)
        {
            if (!_cache.TryGetValue(key, out CacheObject<T>? cacheObject)) return StatusCode.NotFound;

            bool hasValue = cacheObject.TryPeekValue(out T? staleValue);
            if (cacheObject.TryGetValue(out T? validLease)) return validLease;

            _cache.TryRemove(key, out _);
            return hasValue ? new Option<T>(true, staleValue!, StatusCode.Conflict) : StatusCode.NotFound;
        }
    }

    public void AddOrUpdate(string key, T value)
    {
        key.NotEmpty();

        lock (_lock)
        {
            _cache.AddOrUpdate(
                key,
                new CacheObject<T>(_lifeTime).Set(value),
                (_, current) => current.Set(value));
        }
    }

    public void Clear(string path)
    {
        path.NotEmpty();

        lock (_lock)
        {
            _cache.TryRemove(path, out _);
        }
    }
}
