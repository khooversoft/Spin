using System.Collections;
using Toolbox.Tools;

namespace Toolbox.Types;

public class ConcurrentSequence<T> : ICollection<T>, IDisposable
{
    private readonly List<T> _list;
    private readonly ReaderWriterLockSlim _lock = new(LockRecursionPolicy.NoRecursion);
    private bool _disposed;

    public ConcurrentSequence() => _list = new List<T>();

    public ConcurrentSequence(IEnumerable<T> values)
    {
        if (values is null) throw new ArgumentNullException(nameof(values));
        _list = new List<T>(values);
    }

    public int Count
    {
        get
        {
            _lock.EnterReadLock();
            try { return _list.Count; }
            finally { _lock.ExitReadLock(); }
        }
    }

    public bool IsReadOnly => false;

    public void Add(T item)
    {
        _lock.EnterWriteLock();
        try { _list.Add(item); }
        finally { _lock.ExitWriteLock(); }
    }

    public void AddRange(IEnumerable<T> items)
    {
        items.NotNull();

        _lock.EnterWriteLock();
        try { _list.AddRange(items); }
        finally { _lock.ExitWriteLock(); }
    }

    public void Clear()
    {
        _lock.EnterWriteLock();
        try { _list.Clear(); }
        finally { _lock.ExitWriteLock(); }
    }

    public bool Contains(T item)
    {
        _lock.EnterReadLock();
        try { return _list.Contains(item); }
        finally { _lock.ExitReadLock(); }
    }

    public void CopyTo(T[] array, int arrayIndex)
    {
        if (array is null) throw new ArgumentNullException(nameof(array));
        if (arrayIndex < 0) throw new ArgumentOutOfRangeException(nameof(arrayIndex));

        _lock.EnterReadLock();
        try
        {
            if (array.Length - arrayIndex < _list.Count)
                throw new ArgumentException("Destination array is not long enough.", nameof(array));

            _list.CopyTo(0, array, arrayIndex, _list.Count);
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public bool Remove(T item)
    {
        _lock.EnterWriteLock();
        try { return _list.Remove(item); }
        finally { _lock.ExitWriteLock(); }
    }

    public override bool Equals(object? obj)
    {
        return obj is ConcurrentSequence<T> sequence &&
            Count == sequence.Count &&
            this.SequenceEqual(sequence);
    }

    public override int GetHashCode() => base.GetHashCode();

    public static ConcurrentSequence<T> operator +(ConcurrentSequence<T> sequence, T value)
    {
        sequence.Add(value);
        return sequence;
    }

    public static ConcurrentSequence<T> operator +(ConcurrentSequence<T> sequence, IEnumerable<T> values)
    {
        sequence.AddRange(values);
        return sequence;
    }

    public static ConcurrentSequence<T> operator +(ConcurrentSequence<T> sequence, T[] values)
    {
        sequence.AddRange(values);
        return sequence;
    }

    public static bool operator ==(ConcurrentSequence<T>? left, ConcurrentSequence<T>? right) => EqualityComparer<ConcurrentSequence<T>>.Default.Equals(left, right);
    public static bool operator !=(ConcurrentSequence<T>? left, ConcurrentSequence<T>? right) => !(left == right);

    public static implicit operator ConcurrentSequence<T>(T[] values) => new ConcurrentSequence<T>(values);

    public IEnumerator<T> GetEnumerator()
    {
        // Snapshot for lock-free iteration after copy; preserves insertion order
        T[] snapshot;

        _lock.EnterReadLock();
        try
        {
            snapshot = _list.Count == 0 ? Array.Empty<T>() : _list.ToArray();
        }
        finally
        {
            _lock.ExitReadLock();
        }

        return ((IEnumerable<T>)snapshot).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public void Dispose()
    {
        if (_disposed) return;
        _lock.Dispose();
        _disposed = true;
        GC.SuppressFinalize(this);
    }
}
