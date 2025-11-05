using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Toolbox.Tools;

namespace Toolbox.Data;

public interface INonUniqueIndexAccess<TKey, TValue>
{
    bool TryGetValue(TKey key, [NotNullWhen(true)] out IReadOnlyList<TValue>? values);
}

public class NonUniqueIndexProvider<TKey, TValue> : IIndexedCollectionProvider<TValue>, INonUniqueIndexAccess<TKey, TValue>
    where TKey : notnull
    where TValue : notnull
{
    private Func<TValue, TKey> _keySelector;
    private OneToManyIndex<TKey, TValue> _index;

    public NonUniqueIndexProvider(Func<TValue, TKey> keySelector, IEqualityComparer<TKey>? comparer = null)
    {
        _keySelector = keySelector.NotNull();
        _index = new OneToManyIndex<TKey, TValue>(comparer);
    }

    public void Clear() => _index.Clear();
    public void Set(TValue item) => _index.Set(_keySelector(item), item);
    public void Remove(TValue item) => _index.Remove(_keySelector(item), item);

    public bool TryGetValue(TKey key, [NotNullWhen(true)] out IReadOnlyList<TValue>? values)
    {
        if (!_index.TryGetValue(key, out var list) || list.Count == 0)
        {
            values = null;
            return false;
        }

        values = list.ToImmutableArray();
        return true;
    }
}
