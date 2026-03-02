using Toolbox.Extensions;
using Toolbox.Tools;

namespace Toolbox.Data;

public partial class EdgeIndex
{
    public IReadOnlyList<Edge> GetByFrom(string fromKey, string? edgeType = null)
    {
        Func<Edge[]> noWildcard = () => _fromEdges.Lookup(fromKey)
            .Select(x => _edges[x])
            .Where(x => edgeType == null || x.EdgeType.EqualsIgnoreCase(edgeType))
            .ToArray();

        Func<Edge[]> filterList = () => _edges.Values
            .Where(x => x.FromKey.Match(fromKey))
            .Where(x => edgeType == null || x.EdgeType.EqualsIgnoreCase(edgeType))
            .ToArray();

        var list = fromKey.HasWildCard() switch
        {
            false => SelectEdges(noWildcard),
            true => SelectEdges(filterList),
        };

        return list;
    }

    public IReadOnlyList<Edge> GetByTo(string toKey, string? edgeType = null)
    {
        Func<Edge[]> noWildcard = () => _toEdges.Lookup(toKey)
            .Select(x => _edges[x])
            .Where(x => edgeType == null || x.EdgeType.EqualsIgnoreCase(edgeType))
            .ToArray();

        Func<Edge[]> filterList = () => _edges.Values
            .Where(x => x.ToKey.Match(toKey))
            .Where(x => edgeType == null || x.EdgeType.EqualsIgnoreCase(edgeType))
            .ToArray();

        var list = toKey.HasWildCard() switch
        {
            false => SelectEdges(noWildcard),
            true => SelectEdges(filterList),
        };

        return list;
    }

    public IReadOnlyList<Edge> GetByType(string type, string? edgeType = null)
    {
        Func<Edge[]> noWildcard = () => _typeEdges.Lookup(type)
            .Select(x => _edges[x])
            .Where(x => edgeType == null || x.EdgeType.EqualsIgnoreCase(edgeType))
            .ToArray();

        Func<Edge[]> filterList = () => _edges.Values
            .Where(x => x.EdgeType.Match(type))
            .Where(x => edgeType == null || x.EdgeType.EqualsIgnoreCase(edgeType))
            .ToArray();

        var list = type.HasWildCard() switch
        {
            false => SelectEdges(noWildcard),
            true => SelectEdges(filterList),
        };

        return list;
    }

    public bool TryGetValue(string edgeKey, out Edge? value)
    {
        _lock.EnterReadLock();
        try { return _edges.TryGetValue(edgeKey.NotEmpty(), out value); }
        finally { _lock.ExitReadLock(); }
    }

    public bool TryGetValue(string fromKey, string toKey, string type, out Edge? value)
    {
        _lock.EnterReadLock();
        try
        {
            var edgeKey = Edge.CreateKey(fromKey, toKey, type);
            return _edges.TryGetValue(edgeKey, out value);
        }
        finally { _lock.ExitReadLock(); }
    }
}
