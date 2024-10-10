using Toolbox.Data;
using Toolbox.Extensions;
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
    public TagIndex(IEqualityComparer<TPrimaryKey>? primaryKeyComparer = null) => _index = new SecondaryIndex<string, TPrimaryKey>(StringComparer.OrdinalIgnoreCase, primaryKeyComparer);

    public void Clear() => _index.Clear();
    public bool Remove(TPrimaryKey primarykey) => _index.RemovePrimaryKey(primarykey);
    public IReadOnlyList<TPrimaryKey> Lookup(string key) => _index.Lookup(key);
    public IReadOnlyList<string> LookupPrimaryKey(TPrimaryKey pkey) => _index.LookupPrimaryKey(pkey);
    public void Set(TPrimaryKey primaryKey, IReadOnlyDictionary<string, string?> tags)
    {
        _index.RemovePrimaryKey(primaryKey);
        tags.NotNull().ForEach(x => _index.Set(x.Key, primaryKey));
    }
}
