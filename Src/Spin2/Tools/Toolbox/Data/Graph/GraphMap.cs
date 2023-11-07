using System.Collections;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Data;

public class GraphMap : IEnumerable<IGraphCommon>
{
    private readonly GraphNodeIndex _nodes;
    private readonly GraphEdgeIndex _edges;
    private readonly object _lock = new object();

    public GraphMap()
    {
        _nodes = new GraphNodeIndex(_lock, x => _edges.NotNull().Remove(x.Key));
        _edges = new GraphEdgeIndex(_lock, x => _nodes.NotNull().ContainsKey(x));
    }

    public GraphMap(IEnumerable<GraphNode> nodes, IEnumerable<GraphEdge> edges)
        : this()
    {
        nodes.NotNull().ForEach(x => Nodes.Add(x).ThrowOnError("Node add failed"));
        edges.NotNull().ForEach(x => Edges.Add(x).ThrowOnError("Edge add failed"));
    }

    public GraphNodeIndex Nodes => _nodes;
    public GraphEdgeIndex Edges => _edges;

    public GraphMap Add(IGraphCommon element)
    {
        switch (element)
        {
            case GraphNode node: _nodes.Add(node).ThrowOnError("Node add failed"); break;
            case GraphEdge edge: _edges.Add(edge).ThrowOnError("Edge add failed"); break;
            default: throw new ArgumentException("Unknown element");
        }

        return this;
    }

    public void Clear()
    {
        lock (_lock)
        {
            _nodes.Clear();
            _edges.Clear();
        }
    }

    public GraphMap Clone() => new GraphMap(_nodes, _edges);
    public GraphQuery Query() => new GraphQuery(this, _lock);

    public IEnumerator<IGraphCommon> GetEnumerator()
    {
        foreach (var item in Nodes.OfType<IGraphCommon>()) yield return item;
        foreach (var item in Edges.OfType<IGraphCommon>()) yield return item;
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public static GraphMap FromJson(string json) =>
        json.ToObject<GraphSerialization>()
        .NotNull()
        .FromSerialization();
}
