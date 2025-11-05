using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Immutable;

namespace Toolbox.Data;

public class ConcurrentHashSet<T> : IEnumerable<T> where T : notnull
{
    private readonly ConcurrentDictionary<T, byte> _dict;

    public ConcurrentHashSet() => _dict = new();
    public ConcurrentHashSet(IEqualityComparer<T>? comparer) => _dict = new(comparer);

    public int Count => _dict.Count;

    public bool TryAdd(T item) => _dict.TryAdd(item, 0);
    public bool TryRemove(T item) => _dict.TryRemove(item, out _);
    public bool Contains(T item) => _dict.ContainsKey(item);
    public void Clear() => _dict.Clear();
    public ImmutableArray<T> ToImmutableArray() => _dict.Keys.ToImmutableArray();
    public bool IsEmpty => _dict.IsEmpty;

    public IEnumerator<T> GetEnumerator() => _dict.Keys.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => _dict.Keys.GetEnumerator();
}
