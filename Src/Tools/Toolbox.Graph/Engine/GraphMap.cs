using System.Collections;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

public class GraphMap : IEnumerable<IGraphCommon>
{
    private readonly Guid _instance = Guid.NewGuid();
    private readonly object _lock = new object();

    public GraphMap()
    {
        Nodes = new GraphNodeIndex(this, _lock);
        Edges = new GraphEdgeIndex(this, _lock);
    }

    public GraphMap(GraphMapCounter mapCounters)
    {
        mapCounters.NotNull();

        Nodes = new GraphNodeIndex(this, _lock, mapCounters);
        Edges = new GraphEdgeIndex(this, _lock, mapCounters: mapCounters);
    }

    public GraphMap(IEnumerable<GraphNode> nodes, IEnumerable<GraphEdge> edges)
        : this()
    {
        LoadRowsAndEdges(nodes, edges);
    }

    public GraphMap(IEnumerable<GraphNode> nodes, IEnumerable<GraphEdge> edges, GraphMapCounter mapCounters)
        : this(mapCounters)
    {
        LoadRowsAndEdges(nodes, edges);
    }

    public GraphNodeIndex Nodes { get; }
    public GraphEdgeIndex Edges { get; }

    public GraphMap Add(IGraphCommon element)
    {
        switch (element)
        {
            case GraphNode node: Nodes.Add(node).ThrowOnError("Node add failed"); break;
            case GraphEdge edge: Edges.Add(edge).ThrowOnError("Edge add failed"); break;
            default: throw new ArgumentException("Unknown element");
        }

        return this;
    }

    public void UpdateCounters()
    {
        Nodes.UpdateCounters();
        Edges.UpdateCounters();
    }

    public IEnumerator<IGraphCommon> GetEnumerator()
    {
        foreach (var item in Nodes.OfType<IGraphCommon>()) yield return item;
        foreach (var item in Edges.OfType<IGraphCommon>()) yield return item;
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    private void LoadRowsAndEdges(IEnumerable<GraphNode> nodes, IEnumerable<GraphEdge> edges)
    {
        nodes.NotNull().ForEach(x => Nodes.Add(x).ThrowOnError("Node add failed"));
        edges.NotNull().ForEach(x => Edges.Add(x).ThrowOnError("Edge add failed"));
    }
}

public static class GraphMapTool
{
    public static GraphMap Clone(this GraphMap subject) => new GraphMap(subject.Nodes, subject.Edges);
    public static GraphMap FromJson(string json) => json.NotEmpty().ToObject<GraphSerialization>().NotNull().FromSerialization();
}
