using System.Collections.Immutable;

namespace Toolbox.Graph;

// NodeKey => UniqueIndex
internal class NodeKeyIndex
{
    private Dictionary<string, HashSet<UniqueIndex>> _index = new(StringComparer.OrdinalIgnoreCase);

    public void Set(UniqueIndex uniqueIndex)
    {
        if (!_index.TryGetValue(uniqueIndex.NodeKey, out var nodeKeyLookup))
        {
            nodeKeyLookup = new HashSet<UniqueIndex>(UniqueIndexComparer.Default);
            _index[uniqueIndex.NodeKey] = nodeKeyLookup;
        }

        nodeKeyLookup.Add(uniqueIndex);
    }

    public IReadOnlyList<UniqueIndex> Lookup(string nodeKey)
    {
        if (!_index.TryGetValue(nodeKey, out var nodeKeyLookup)) return Array.Empty<UniqueIndex>();

        return nodeKeyLookup.ToImmutableArray();
    }

    public bool Remove(string nodeKey) => _index.Remove(nodeKey);

    public bool Remove(UniqueIndex uniqueIndex)
    {
        if (!_index.TryGetValue(uniqueIndex.NodeKey, out var nodeKeyLookup)) return false;

        var status = nodeKeyLookup.Remove(uniqueIndex);

        if (nodeKeyLookup.Count == 0)
        {
            _index.Remove(uniqueIndex.NodeKey);
        }

        return status;
    }
}
