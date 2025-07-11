using Toolbox.Extensions;
using Toolbox.Types;

namespace Toolbox.Graph;

/// <summary>
/// "{tagKey}={tagValue} = primary key
/// </summary>
internal class TagValueIndex
{
    private readonly Dictionary<string, UniqueIndex> _index = new(StringComparer.OrdinalIgnoreCase);
    private readonly NodeKeyIndex _nodeKeyLookup = new NodeKeyIndex();  // Lookup by nodeKey

    public Option Add(UniqueIndex uniqueIndex)
    {
        if (!_index.TryAdd(uniqueIndex.PrimaryKey, uniqueIndex)) return (StatusCode.Conflict, $"Index={uniqueIndex.PrimaryKey} already exist");
        _nodeKeyLookup.Set(uniqueIndex);
        return StatusCode.OK;
    }

    public Option Add(string indexName, string value, string nodeKey)
    {
        var uniqueIndex = new UniqueIndex(indexName, value, nodeKey);
        _nodeKeyLookup.Set(uniqueIndex);
        return Add(uniqueIndex);
    }

    public void Clear()
    {
        _index.Clear();
        _nodeKeyLookup.Clear();
    }

    public Option<UniqueIndex> Lookup(string indexName, string value)
    {
        string pk = UniqueIndexComparer.CreatePrimaryKey(indexName, value);
        if (!_index.TryGetValue(pk, out var indexValue)) return StatusCode.NotFound;
        return indexValue;
    }

    public IReadOnlyList<UniqueIndex> LookupByNodeKey(string nodeKey) => _nodeKeyLookup.Lookup(nodeKey);

    public Option Remove(string indexName, string value)
    {
        string pk = UniqueIndexComparer.CreatePrimaryKey(indexName, value);

        if (!_index.TryGetValue(pk, out var uniqueIndex)) return StatusCode.NotFound;
        var status = _index.Remove(pk) switch
        {
            true => StatusCode.OK,
            _ => StatusCode.NotFound,
        };

        _nodeKeyLookup.Remove(uniqueIndex);
        return status;
    }

    public Option RemoveNodeKey(string nodeKey)
    {
        var uniqueIndices = _nodeKeyLookup.Lookup(nodeKey);
        if (uniqueIndices == null) return StatusCode.NotFound;

        uniqueIndices.ForEach(x => _index.Remove(x.PrimaryKey));

        return _nodeKeyLookup.Remove(nodeKey) switch
        {
            true => StatusCode.OK,
            _ => StatusCode.NotFound,
        };
    }
}