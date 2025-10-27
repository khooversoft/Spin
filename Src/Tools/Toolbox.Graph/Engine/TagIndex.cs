using Toolbox.Data;
using Toolbox.Tools;

namespace Toolbox.Graph;

/// <summary>
/// Tag indexer
///   Normal index = Tags => NodeKey or EdgeKey, no restrictions
///   Node Unique index = Tags => NodeKey is unique
/// </summary>
/// <typeparam name="TPrimaryKey"></typeparam>
internal class TagIndex<TPrimaryKey> where TPrimaryKey : notnull
{
    private readonly SecondaryIndex<string, TPrimaryKey> _index;
    public TagIndex(IEqualityComparer<TPrimaryKey>? primaryKeyComparer = null) =>
        _index = new SecondaryIndex<string, TPrimaryKey>(StringComparer.OrdinalIgnoreCase, primaryKeyComparer);

    public void Clear() => _index.Clear();
    public bool Remove(TPrimaryKey primaryKey) => _index.RemovePrimaryKey(primaryKey);
    public IReadOnlyList<TPrimaryKey> Lookup(string key) => _index.Lookup(key);
    public IReadOnlyList<string> LookupPrimaryKey(TPrimaryKey pkey) => _index.LookupPrimaryKey(pkey);

    public void Set(TPrimaryKey primaryKey, IReadOnlyDictionary<string, string?> tags)
    {
        _index.RemovePrimaryKey(primaryKey);

        foreach (var item in tags.NotNull()) _index.Set(item.Key, primaryKey);
    }
}
