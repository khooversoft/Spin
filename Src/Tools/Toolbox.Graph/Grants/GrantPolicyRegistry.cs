using Microsoft.Extensions.Logging;
using Toolbox.Data;
using Toolbox.Extensions;
using Toolbox.Telemetry;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

public class GrantPolicyRegistry
{
    private readonly ILogger _logger;
    private readonly Func<GraphCore?> _getGraph;
    private readonly ReaderWriterLockSlim _gate = new();
    private readonly ITelemetry? _telemetry;

    public GrantPolicyRegistry(Func<GraphCore?> getGraph, ILogger logger, ITelemetry? telemetry = null)
    {
        _getGraph = getGraph.NotNull();
        _logger = logger.NotNull();
        _telemetry = telemetry;
    }

    public Option AddOrUpdate(GrantPolicy grantPolicy)
    {
        _gate.EnterWriteLock();
        try
        {
            var graph = _getGraph().NotNull("Graph not loaded");
            grantPolicy.NotNull().Validate().ThrowOnError();

            if (!graph.Nodes.TryGetValue(grantPolicy.NodeKey, out var node)) return StatusCode.NotFound;

            node = node.NotNull() with { Payload = grantPolicy.ToDataETag() };
            var result = graph.Nodes.AddOrUpdate(node);
            return result;
        }
        finally { _gate.ExitWriteLock(); }
    }

    public Option<GrantPolicy> Get(string nodeKey)
    {
        _gate.EnterReadLock();
        try
        {
            var graph = _getGraph().NotNull("Graph not loaded");

            var resolvedNodeKey = NodeTool.CreateKey(nodeKey, GrantPolicy.NodeType);
            if (!graph.Nodes.TryGetValue(resolvedNodeKey, out var node)) return StatusCode.NotFound;

            var result = node.Payload.ToObject<GrantPolicy>();
            return result;
        }
        finally { _gate.ExitReadLock(); }
    }

    public IReadOnlyList<GrantPolicy> GetAll()
    {
        _gate.EnterReadLock();
        try
        {
            var graph = _getGraph().NotNull("Graph not loaded");

            var items = graph.Nodes
                .Where(x => NodeTool.ParseKey(x.NodeKey).NodeType.EqualsIgnoreCase(GrantPolicy.NodeType))
                .Select(x => x.Payload.ToObject<GrantPolicy>())
                .ToArray();

            return items;
        }
        finally { _gate.ExitReadLock(); }
    }

    public Option Remove(string groupName)
    {
        _gate.EnterWriteLock();
        try
        {
            var graph = _getGraph().NotNull("Graph not loaded");
            var result = graph.Nodes.Remove(groupName);
            return result;
        }
        finally { _gate.ExitWriteLock(); }
    }

    public Option TryAdd(GrantPolicy grantPolicy)
    {
        _gate.EnterWriteLock();
        try
        {
            var graph = _getGraph().NotNull("Graph not loaded");
            grantPolicy.NotNull().Validate().ThrowOnError();

            var node = new Node(grantPolicy.NodeKey, grantPolicy.ToDataETag());
            var addOption = graph.Nodes.TryAdd(node);
            return addOption;
        }
        finally { _gate.ExitWriteLock(); }
    }
}
