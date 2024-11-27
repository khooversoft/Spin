namespace Toolbox.Tools;

/// <summary>
/// Cache object, valid for only a specific amount of time specified in lifetime.
/// If specified, refresh, can indicate when a refresh operations should be done.
/// 
/// This object is thread protected by using non-locking methods.
/// </summary>
/// <typeparam name="T">type of object cached</typeparam>
public class CacheObject<T>
{
    private ValueStore? _valueStore;
    private readonly object _lock = new object();

    public CacheObject(TimeSpan lifeTime) => LifeTime = lifeTime;

    public TimeSpan LifeTime { get; }

    public CacheObject<T> Clear()
    {
        lock (_lock)
        {
            _valueStore = null;
            return this;
        }
    }

    public T Value => TryGetValue(out var value) ? value : throw new InvalidOperationException("Cache is not valid");

    public bool IsValid() => _valueStore switch
    {
        null => false,
        var v => DateTimeOffset.Now < v.Value.ValidTo
    };

    public bool TryGetValue(out T value)
    {
        lock (_lock)
        {
            value = default!;

            ValueStore? current = _valueStore;
            if (current == null) return false;

            if (DateTimeOffset.Now < current.Value.ValidTo)
            {
                value = current.Value.Value;
                return true;
            }

            _valueStore = default;
            return false;
        }
    }

    public CacheObject<T> Set(T? value)
    {
        lock (_lock)
        {
            _valueStore = value != null ? new ValueStore(value, DateTime.Now + LifeTime) : null;
            return this;
        }
    }

    private readonly struct ValueStore(T value, DateTime validTo)
    {
        public DateTime ValidTo { get; } = validTo;
        public T Value { get; } = value;
    }
}


public static class CacheObjectExtensions
{
    public static CacheObject<T> ToCacheObject<T>(this T value, TimeSpan lifeTime) => new CacheObject<T>(lifeTime).Set(value);
}
