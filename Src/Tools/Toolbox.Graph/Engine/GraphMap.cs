using System.Collections;
using Microsoft.Extensions.Logging;
using Toolbox.Telemetry;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

public class GraphMap : IEnumerable<IGraphCommon>
{
    private readonly object _lock = new object();
    private readonly ILogger<GraphMap> _logger;
    private readonly ITelemetry? _telemetry;

    public GraphMap(ILogger<GraphMap> logger, ITelemetry? telemetry = null)
    {
        _logger = logger.NotNull();
        _telemetry = telemetry;
        Nodes = new GraphNodeIndex(this, _lock, telemetry);
        Edges = new GraphEdgeIndex(this, _lock, telemetry);
    }

    public GraphMap(GraphSerialization graphSerialization, ILogger<GraphMap> logger, ITelemetry? telemetry = null)
    {
        _logger = logger.NotNull();
        _telemetry = telemetry;

        Nodes = new GraphNodeIndex(this, _lock, telemetry);
        Edges = new GraphEdgeIndex(this, _lock, telemetry);

        graphSerialization.NotNull();
        LoadRowsAndEdges(graphSerialization.Nodes, graphSerialization.Edges);
        LastLogSequenceNumber = graphSerialization.LastLogSequenceNumber;

        foreach (var item in graphSerialization.PrincipalIdentities) GrantControl.Principals.Add(item);
        foreach (var item in graphSerialization.SecurityGroups) GrantControl.Groups.Add(item);
    }

    public string? LastLogSequenceNumber { get; private set; } = null;
    public GraphNodeIndex Nodes { get; }
    public GraphEdgeIndex Edges { get; }
    public GrantControl GrantControl { get; } = new GrantControl();

    public void SetLastLogSequenceNumber(string lsn) => LastLogSequenceNumber = lsn.NotEmpty();

    public GraphMap Add(IGraphCommon element)
    {
        switch (element)
        {
            case GraphNode node:
                Nodes.Add(node).ThrowOnError("Node add failed");
                break;
            case GraphEdge edge:
                Edges.Add(edge).ThrowOnError("Edge add failed");
                break;
            default:
                throw new ArgumentException("Unknown element");
        }

        return this;
    }

    public GraphMap Clone()
    {
        var serialization = this.ToSerialization();
        return new GraphMap(serialization, _logger, _telemetry);
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
