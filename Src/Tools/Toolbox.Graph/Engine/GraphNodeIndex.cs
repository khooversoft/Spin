using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

[assembly: InternalsVisibleTo("Toolbox.Graph.test")]

namespace Toolbox.Graph;

public class GraphNodeIndex : IEnumerable<GraphNode>
{
    private readonly Dictionary<string, GraphNode> _index;
    private readonly object _lock;
    private readonly GraphRI _graphRI;

    internal GraphNodeIndex(object syncLock, GraphRI graphRI)
    {
        _lock = syncLock.NotNull();
        _graphRI = graphRI.NotNull();
        _index = new Dictionary<string, GraphNode>(StringComparer.OrdinalIgnoreCase);
    }

    public GraphNode this[string key]
    {
        get => _index[key];
        internal set => Set(value).ThrowOnError();
    }

    public int Count => _index.Count;
    public bool ContainsKey(string key) => _index.ContainsKey(key);
    public bool TryGetValue(string key, [NotNullWhen(true)] out GraphNode? value) => _index.TryGetValue(key, out value);


    internal Option Add(GraphNode node, IGraphTrxContext? graphContext = null)
    {
        if (!node.Validate(out var v)) return v;

        lock (_lock)
        {
            Option option = _index.TryAdd(node.Key, node) switch
            {
                true => StatusCode.OK,
                false => (StatusCode.Conflict, $"Node key={node.Key} already exist"),
            };

            if (option.IsOk())
            {
                graphContext?.ChangeLog.Push(new CmNodeAdd(node));
            }

            return option;
        }
    }

    internal void Clear()
    {
        lock (_lock)
        {
            _index.Clear();
        }
    }

    internal Option Remove(string key, IGraphTrxContext? graphContext = null)
    {
        lock (_lock)
        {
            if (!_index.Remove(key, out var oldValue)) return StatusCode.NotFound;

            _graphRI.RemovedNodeFromEdges(oldValue!, graphContext);
            graphContext?.ChangeLog.Push(new CmNodeDelete(oldValue));
            return StatusCode.OK;
        }
    }

    internal Option Set(GraphNode node, IGraphTrxContext? graphContext = null)
    {
        if (!node.Validate(out var v)) return v;

        lock (_lock)
        {
            bool exist = _index.TryGetValue(node.Key, out GraphNode? current);

            _index[node.Key] = node;
            graphContext?.ChangeLog.Push(exist switch
            {
                false  => new CmNodeAdd(node),
                true => new CmNodeChange(current!, node),
            });

            return StatusCode.OK;
        }
    }

    public IEnumerator<GraphNode> GetEnumerator() => _index.Values.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
