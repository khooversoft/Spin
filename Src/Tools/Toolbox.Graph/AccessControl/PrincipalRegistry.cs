using Microsoft.Extensions.Logging;
using Toolbox.Data;
using Toolbox.Extensions;
using Toolbox.Telemetry;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

public class PrincipalRegistry
{
    private readonly ILogger _logger;
    private readonly Func<GraphCore?> _getGraph;
    private readonly ReaderWriterLockSlim _gate = new();
    private readonly ITelemetry? _telemetry;

    public PrincipalRegistry(Func<GraphCore?> getGraph, ILogger logger, ITelemetry? telemetry = null)
    {
        _getGraph = getGraph.NotNull();
        _logger = logger.NotNull();
        _telemetry = telemetry;
    }

    public Option AddOrUpdate(PrincipalIdentity principalIdentity)
    {
        principalIdentity.NotNull().Validate().ThrowOnError();
        var graph = _getGraph().NotNull("Graph not loaded");
        var node = new Node(principalIdentity.NodeKey, principalIdentity.ToDataETag());

        _gate.EnterWriteLock();
        try
        {
            var result = graph.Nodes.AddOrUpdate(node);
            if (result.IsError()) return result;

            var updateOption = AddOrUpdateReferenceNode(graph, principalIdentity);
            if (updateOption.IsError()) return updateOption;

            return result;
        }
        finally { _gate.ExitWriteLock(); }
    }

    public Option<PrincipalIdentity> Get(string principalId)
    {
        _gate.EnterReadLock();
        try
        {
            var graph = _getGraph().NotNull("Graph not loaded");

            var nodeKey = NodeTool.CreateKey(principalId, PrincipalIdentity.NodeType);
            if (!graph.Nodes.TryGetValue(nodeKey, out var node)) return StatusCode.NotFound;

            var result = node.NotNull().Payload.ToObject<PrincipalIdentity>();
            return result;
        }
        finally { _gate.ExitReadLock(); }
    }

    public IReadOnlyList<PrincipalIdentity> GetAll()
    {
        _gate.EnterReadLock();
        try
        {
            var graph = _getGraph().NotNull("Graph not loaded");

            var items = graph.Nodes
                .Where(x => NodeTool.ParseKey(x.NodeKey).NodeType == PrincipalIdentity.NodeType)
                .Select(x => x.Payload.ToObject<PrincipalIdentity>())
                .ToArray();

            return items;
        }
        finally { _gate.ExitReadLock(); }
    }

    public Option Remove(string principalId)
    {
        _gate.EnterWriteLock();
        try
        {
            var graph = _getGraph().NotNull("Graph not loaded");
            var nodeKey = NodeTool.CreateKey(principalId, PrincipalIdentity.NodeType);

            // Get a list of refernece nodes
            var referenceNodes = graph.Nodes.GetNodes(nodeKey)
                .SelectMany(x => graph.Edges.GetByFrom(x.NodeKey, PrincipalIdentity.NodeReferenceType))
                .SelectMany(x => graph.Nodes.GetNodes(x.ToKey))
                .ToArray();

            var result = graph.Nodes.Remove(nodeKey);
            if (result.IsError()) return result;

            foreach (var node in referenceNodes) graph.Nodes.Remove(node.NodeKey);
            return result;
        }
        finally { _gate.ExitWriteLock(); }
    }

    public Option TryAdd(PrincipalIdentity principalIdentity)
    {
        _gate.EnterWriteLock();
        try
        {
            var graph = _getGraph().NotNull("Graph not loaded");
            principalIdentity.NotNull().Validate().ThrowOnError();

            if (graph.Nodes.TryGetValue(principalIdentity.NodeKey, out _)) return StatusCode.Conflict;

            var result = graph.Nodes.Add(principalIdentity.NodeKey, principalIdentity.ToDataETag());
            if (result.IsError()) return result;

            var updateOption = AddOrUpdateReferenceNode(graph, principalIdentity);
            if (updateOption.IsError()) return updateOption;

            return result;
        }
        finally { _gate.ExitWriteLock(); }
    }

    /// <summary>
    /// Reference nodes are owned by parent (i.e. 'FromKey' in edges)
    /// </summary>
    private Option AddOrUpdateReferenceNode(GraphCore graph, PrincipalIdentity principalIdentity)
    {
        var currentReferenceKeys = graph.Nodes.GetNodes(principalIdentity.NodeKey)
            .SelectMany(x => graph.Edges.GetByFrom(x.NodeKey, PrincipalIdentity.NodeReferenceType))
            .SelectMany(x => graph.Nodes.GetNodes(x.ToKey))
            .ToArray();

        var shouldBeReferenceKeys = new string[]
        {
            principalIdentity.CreateNameIdentifierNodeKey(),
            principalIdentity.CreateUserNameNodeKey(),
            principalIdentity.CreateEmailNodeKey()
        }.ToArray();

        // Delete edges
        var deleteNodes = currentReferenceKeys.Select(n => n.NodeKey)
            .Except(shouldBeReferenceKeys)
            .ToArray();
        foreach (var nodeKey in deleteNodes) graph.Nodes.Remove(nodeKey);

        // Add missing
        var missingNodes = shouldBeReferenceKeys
            .Except(currentReferenceKeys.Select(n => n.NodeKey))
            .ToArray();

        foreach (var nodeKey in missingNodes)
        {
            var refLinkPayload = new NodeReference(principalIdentity.NodeKey);

            var node = new Node(nodeKey, refLinkPayload.ToDataETag());
            var result = graph.Nodes.Add(node.NodeKey, node.Payload);
            if (result.IsError()) return result;

            var edge = new Edge(principalIdentity.NodeKey, node.NodeKey, PrincipalIdentity.NodeReferenceType);
            var edgeResult = graph.Edges.TryAdd(edge);
            if (edgeResult.IsError()) return edgeResult;
        }

        return StatusCode.OK;
    }
}
