namespace Toolbox.Types;

/// <summary>
/// Thread-safe, time-bound cache container for a single value.
/// Use <see cref="Set"/> to store a value with the configured lifetime, <see cref="TryGetValue"/> to read only if still valid,
/// and <see cref="TryPeekValue"/> to inspect even if expired (without extending validity).
/// </summary>
public class CacheObject<T>
{
    private ValueStore? _valueStore;
    private readonly object _lock = new object();

    /// <summary>
    /// Creates a cache with a fixed lifetime for stored values.
    /// </summary>
    /// <param name="lifeTime">Duration a stored value remains valid.</param>
    public CacheObject(TimeSpan lifeTime) => LifeTime = lifeTime;

    /// <summary>
    /// Gets the configured lifetime for cached values.
    /// </summary>
    public TimeSpan LifeTime { get; }

    /// <summary>
    /// Clears the cached value.
    /// </summary>
    public CacheObject<T> Clear()
    {
        lock (_lock)
        {
            _valueStore = null;
            return this;
        }
    }

    /// <summary>
    /// Gets the cached value if valid; otherwise throws if no valid value exists.
    /// </summary>
    public T Value => TryGetValue(out var value) ? value : throw new InvalidOperationException("Cache is not valid");

    /// <summary>
    /// Determines whether a valid (non-expired) value exists.
    /// </summary>
    public bool IsValid() => _valueStore switch
    {
        null => false,
        var v => DateTimeOffset.Now < v.Value.ValidTo
    };

    /// <summary>
    /// Attempts to get the cached value only if it is still valid.
    /// </summary>
    /// <param name="value">When this method returns, contains the cached value if valid; otherwise default.</param>
    /// <returns><c>true</c> if a valid value was returned; otherwise <c>false</c>.</returns>
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

    /// <summary>
    /// Attempts to read the cached value even if it is expired. Does not update validity.
    /// </summary>
    /// <param name="value">When this method returns, contains the cached value if present; otherwise default.</param>
    /// <returns><c>true</c> if a value was present; otherwise <c>false</c>.</returns>
    public bool TryPeekValue(out T value)
    {
        lock (_lock)
        {
            value = default!;
            ValueStore? current = _valueStore;
            if (current == null) return false;

            value = current.Value.Value;
            return true;
        }
    }

    /// <summary>
    /// Stores a value and sets its expiration to now + <see cref="LifeTime"/>. Passing null clears the cache.
    /// </summary>
    /// <param name="value">Value to cache.</param>
    /// <returns>The current instance for chaining.</returns>
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

/// <summary>
/// Extension helpers for creating cache objects.
/// </summary>
public static class CacheObjectExtensions
{
    /// <summary>
    /// Wraps a value into a <see cref="CacheObject{T}"/> with the provided lifetime.
    /// </summary>
    public static CacheObject<T> ToCacheObject<T>(this T value, TimeSpan lifeTime) => new CacheObject<T>(lifeTime).Set(value);
}
