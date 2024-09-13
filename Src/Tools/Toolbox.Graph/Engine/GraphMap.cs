using System.Collections;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

public class GraphMap : IEnumerable<IGraphCommon>
{
    private readonly GraphNodeIndex _nodes;
    private readonly GraphEdgeIndex _edges;
    private readonly AsyncReaderWriterLock _rwLock = new AsyncReaderWriterLock();
    private readonly object _lock = new object();
    private readonly GraphRI _graphRI;

    public GraphMap()
    {
        _graphRI = new GraphRI();
        _nodes = new GraphNodeIndex(_lock, _graphRI);
        _edges = new GraphEdgeIndex(_lock, _graphRI);
        _graphRI.GraphNodeIndex = _nodes;
        _graphRI.GraphEdgeIndex = _edges;
    }

    public GraphMap(IEnumerable<GraphNode> nodes, IEnumerable<GraphEdge> edges)
        : this()
    {
        nodes.NotNull().ForEach(x => Nodes.Add(x).ThrowOnError("Node add failed"));
        edges.NotNull().ForEach(x => Edges.Add(x).ThrowOnError("Edge add failed"));
    }

    internal AsyncReaderWriterLock ReadWriterLock => _rwLock;
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

public static class GraphMapExtensions
{
    public static GraphMap Clone(this GraphMap subject) => new GraphMap(subject.Nodes, subject.Edges);
}
