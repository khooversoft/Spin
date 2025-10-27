using System.Collections;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

public class GraphMap : IEnumerable<IGraphCommon>
{
    private readonly object _lock = new object();

    public GraphMap()
    {
        Counters = new GraphMapCounter();
        Nodes = new GraphNodeIndex(this, _lock, Counters);
        Edges = new GraphEdgeIndex(this, _lock, mapCounters: Counters);
    }

    public GraphMap(GraphMapCounter mapCounters)
    {
        Counters = mapCounters.NotNull();
        Nodes = new GraphNodeIndex(this, _lock, Counters);
        Edges = new GraphEdgeIndex(this, _lock, mapCounters: Counters);
    }

    public GraphMap(IEnumerable<GraphNode> nodes, IEnumerable<GraphEdge> edges)
        : this()
    {
        LoadRowsAndEdges(nodes, edges);
    }

    public GraphMap(GraphSerialization graphSerialization, GraphMapCounter mapCounters)
        : this(mapCounters)
    {
        graphSerialization.NotNull();
        LoadRowsAndEdges(graphSerialization.Nodes, graphSerialization.Edges);
        LastLogSequenceNumber = graphSerialization.LastLogSequenceNumber;

        foreach (var item in graphSerialization.PrincipalIdentities) GrantControl.Principals.Add(item);
        foreach (var item in graphSerialization.SecurityGroups) GrantControl.Groups.Add(item);
    }

    public GraphMap(IEnumerable<GraphNode> nodes, IEnumerable<GraphEdge> edges, GraphMapCounter mapCounters, string? lastLsn = null)
        : this(mapCounters)
    {
        LoadRowsAndEdges(nodes, edges);
        LastLogSequenceNumber = lastLsn;
    }

    public string? LastLogSequenceNumber { get; private set; } = null;
    public GraphMapCounter Counters { get; }
    public GraphNodeIndex Nodes { get; }
    public GraphEdgeIndex Edges { get; }
    public GrantControl GrantControl { get; } = new GrantControl();

    public void SetLastLogSequenceNumber(string lsn) => LastLogSequenceNumber = lsn.NotEmpty();

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
        nodes.NotNull();
        edges.NotNull();

        foreach (var node in nodes) Nodes.Add(node).ThrowOnError("Node add failed");
        foreach (var edge in edges) Edges.Add(edge).ThrowOnError("Edge add failed");
    }
}

public static class GraphMapTool
{
    public static GraphMap Clone(this GraphMap subject) => new GraphMap(subject.Nodes, subject.Edges);
    public static GraphMap Clone(this GraphMap subject, GraphMapCounter counters) => new GraphMap(subject.Nodes, subject.Edges, counters);
    public static GraphMap FromJson(string json) => json.NotEmpty().ToObject<GraphSerialization>().NotNull().FromSerialization();
}
