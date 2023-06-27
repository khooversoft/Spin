using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

    public CacheObject(TimeSpan lifeTime) => LifeTime = lifeTime;

    public TimeSpan LifeTime { get; }

    public CacheObject<T> Clear()
    {
        _valueStore = null;
        return this;
    }

    public bool TryGetValue(out T value)
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

    public CacheObject<T> Set(T? value)
    {
        _valueStore = value != null ? new ValueStore(value, DateTime.Now + LifeTime) : null;
        return this;
    }

    public bool IsValid()
    {
        return _valueStore switch
        {
            null => false,
            var v => DateTimeOffset.Now < v.Value.ValidTo
        };
    }

    private struct ValueStore
    {
        public ValueStore(T value, DateTime validTo)
        {
            Value = value;
            ValidTo = validTo;
        }

        public DateTime ValidTo { get; }

        public T Value { get; }
    }
}


public static class CacheObjectExtensions
{
    public static CacheObject<T> ToCacheObject<T>(this T value, TimeSpan lifeTime) => new CacheObject<T>(lifeTime).Set(value);
}
