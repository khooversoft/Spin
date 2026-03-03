using System.Collections;
using Microsoft.Extensions.Logging;
using Toolbox.Data;
using Toolbox.Telemetry;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

public class GraphMap : IEnumerable<IGraphCommon>
{
    private readonly ReaderWriterLockSlim _gate = new();
    private readonly ILogger<GraphMap> _logger;
    private readonly ITelemetry? _telemetry;

    public GraphMap(ILogger<GraphMap> logger, ITelemetry? telemetry = null)
    {
        _logger = logger.NotNull();
        _telemetry = telemetry;

        Nodes = new GraphNodeIndex(this, _logger, telemetry);
        Edges = new GraphEdgeIndex(this, _logger, telemetry);
        GrantControl = new GrantControl(logger, telemetry);
    }

    public GraphMap(GraphSerialization graphSerialization, ILogger<GraphMap> logger, ITelemetry? telemetry = null)
        : this(logger, telemetry)
    {
        graphSerialization.NotNull();
        LoadRowsAndEdges(graphSerialization.Nodes, graphSerialization.Edges);
        LastLogSequenceNumber = graphSerialization.LastLogSequenceNumber;

        GraphCore core = graphSerialization.GrantControl.FromSerialization();
        GrantControl.SetGraph(core);
    }

    public string? LastLogSequenceNumber { get; private set; } = null;
    public GraphNodeIndex Nodes { get; }
    public GraphEdgeIndex Edges { get; }
    public GrantControl GrantControl { get; }

    public void SetLastLogSequenceNumber(string? lsn) => LastLogSequenceNumber = lsn;

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
