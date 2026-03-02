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
    private readonly ReaderWriterLockSlim _slimLock;
    private readonly ITelemetry? _telemetry;

    public GrantPolicyRegistry(Func<GraphCore?> getGraph, ReaderWriterLockSlim slimLock, ILogger logger, ITelemetry? telemetry)
    {
        _getGraph = getGraph.NotNull();
        _logger = logger.NotNull();
        _slimLock = slimLock.NotNull();
        _telemetry = telemetry;
    }

    public Option AddOrUpdate(GrantPolicy grantPolicy)
    {
        _slimLock.EnterWriteLock();
        try
        {
            var graph = _getGraph().NotNull("Graph not loaded");
            grantPolicy.NotNull().Validate().ThrowOnError();

            if (!graph.Nodes.TryGetValue(grantPolicy.NodeKey, out var node)) return StatusCode.NotFound;

            node = node.NotNull() with { Payload = grantPolicy.ToDataETag() };
            var result = graph.Nodes.AddOrUpdate(node);
            return result;
        }
        finally { _slimLock.ExitWriteLock(); }
    }

    public Option<GrantPolicy> Get(string nodeKey)
    {
        _slimLock.EnterReadLock();
        try
        {
            var graph = _getGraph().NotNull("Graph not loaded");

            var resolvedNodeKey = NodeTool.CreateKey(nodeKey, GrantPolicy.NodeType);
            if (!graph.Nodes.TryGetValue(resolvedNodeKey, out var node)) return StatusCode.NotFound;

            var result = node.ToObject<GrantPolicy>();
            return result;
        }
        finally { _slimLock.ExitReadLock(); }
    }

    public IReadOnlyList<GrantPolicy> GetAll()
    {
        _slimLock.EnterReadLock();
        try
        {
            var graph = _getGraph().NotNull("Graph not loaded");

            var items = graph.Nodes
                .Where(x => NodeTool.ParseKey(x.NodeKey).NodeType == GrantPolicy.NodeType)
                .Select(x => x.ToObject<GrantPolicy>())
                .ToArray();

            return items;
        }
        finally { _slimLock.ExitReadLock(); }
    }

    public Option Remove(string groupName)
    {
        _slimLock.EnterWriteLock();
        try
        {
            var graph = _getGraph().NotNull("Graph not loaded");
            var result = graph.Nodes.Remove(groupName);
            return result;
        }
        finally { _slimLock.ExitWriteLock(); }
    }

    public Option TryAdd(GrantPolicy grantPolicy)
    {
        _slimLock.EnterWriteLock();
        try
        {
            var graph = _getGraph().NotNull("Graph not loaded");
            grantPolicy.NotNull().Validate().ThrowOnError();

            if (!graph.Nodes.TryGetValue(grantPolicy.NodeKey, out var node)) return StatusCode.Conflict;

            var addOption = graph.Nodes.Add(grantPolicy.NodeKey, node.ToDataETag());
            return addOption;
        }
        finally { _slimLock.ExitWriteLock(); }
    }
}
