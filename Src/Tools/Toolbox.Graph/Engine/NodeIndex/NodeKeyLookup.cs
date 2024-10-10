namespace Toolbox.Graph;

internal class NodeKeyLookup
{
    // NodeKey => UniqueIndex
    private Dictionary<string, HashSet<UniqueIndex>> _index = new(StringComparer.OrdinalIgnoreCase);

    public void Add(UniqueIndex uniqueIndex)
    {
        if (!_index.TryGetValue(uniqueIndex.NodeKey, out var nodeKeyLookup))
        {
            nodeKeyLookup = new HashSet<UniqueIndex>(new UniqueIndexEqualityComparer());
            _index[uniqueIndex.NodeKey] = nodeKeyLookup;
        }

        nodeKeyLookup.Add(uniqueIndex);
    }

    public IEnumerable<UniqueIndex> Lookup(string nodeKey)
    {
        if (!_index.TryGetValue(nodeKey, out var nodeKeyLookup)) return Array.Empty<UniqueIndex>();

        return nodeKeyLookup;
    }

    public bool Remove(string nodeKey) => _index.Remove(nodeKey);
}
