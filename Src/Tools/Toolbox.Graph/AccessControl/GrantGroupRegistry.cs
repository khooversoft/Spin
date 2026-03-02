//using Microsoft.Extensions.Logging;
//using Toolbox.Data;
//using Toolbox.Extensions;
//using Toolbox.Telemetry;
//using Toolbox.Tools;
//using Toolbox.Types;

//namespace Toolbox.Graph;

//public class GrantGroupRegistry
//{
//    public const string EdgeType = "grantGroupPrincipal";
//    private readonly ILogger _logger;
//    private readonly Func<GraphCore?> _getGraph;
//    private readonly ReaderWriterLockSlim _slimLock;
//    private readonly ITelemetry? _telemetry;

//    public GrantGroupRegistry(Func<GraphCore?> getGraph, ReaderWriterLockSlim slimLock, ILogger logger, ITelemetry? telemetry)
//    {
//        _getGraph = getGraph.NotNull();
//        _logger = logger.NotNull();
//        _slimLock = slimLock.NotNull();
//        _telemetry = telemetry;

//        Principals = new GrantGroupPrincipal(getGraph, slimLock, logger, telemetry);
//    }

//    public GrantGroupPrincipal Principals { get; }

//    public Option Add(GrantGroup groupDetail)
//    {
//        _slimLock.EnterWriteLock();
//        try
//        {
//            var graph = _getGraph().NotNull("Graph not loaded");
//            groupDetail.NotNull().Validate().ThrowOnError();

//            if (!graph.Nodes.TryGetValue(groupDetail.NodeKey, out var node)) return StatusCode.Conflict;

//            var result = graph.Nodes.Add(groupDetail.NodeKey, node.ToDataETag());
//            return result;
//        }
//        finally { _slimLock.ExitWriteLock(); }
//    }

//    public bool Contains(string groupName)
//    {
//        _slimLock.EnterReadLock();
//        try
//        {
//            var graph = _getGraph().NotNull("Graph not loaded");

//            var nodeKey = NodeTool.CreateKey(groupName, GrantGroup.NodeType);
//            return graph.Nodes.ContainsKey(nodeKey);
//        }
//        finally { _slimLock.ExitReadLock(); }
//    }

//    public Option<GrantGroup> Get(string groupName)
//    {
//        _slimLock.EnterReadLock();
//        try
//        {
//            var graph = _getGraph().NotNull("Graph not loaded");

//            var nodeKey = NodeTool.CreateKey(groupName, GrantGroup.NodeType);
//            if (!graph.Nodes.TryGetValue(nodeKey, out var node)) return StatusCode.NotFound;

//            var result = node.ToObject<GrantGroup>();
//            return result;
//        }
//        finally { _slimLock.ExitReadLock(); }
//    }

//    public Option Remove(string groupName)
//    {
//        _slimLock.EnterWriteLock();
//        try
//        {
//            var graph = _getGraph().NotNull("Graph not loaded");
//            var result = graph.Nodes.Remove(groupName);
//            return result;
//        }
//        finally { _slimLock.ExitWriteLock(); }
//    }

//    public Option Update(GrantGroup groupDetail)
//    {
//        _slimLock.EnterWriteLock();
//        try
//        {
//            var graph = _getGraph().NotNull("Graph not loaded");
//            groupDetail.NotNull().Validate().ThrowOnError();

//            if (!graph.Nodes.TryGetValue(groupDetail.Name, out var node)) return StatusCode.NotFound;

//            node = node.NotNull() with { Payload = groupDetail.ToDataETag() };
//            var result = graph.Nodes.AddOrUpdate(node);
//            return result;
//        }
//        finally { _slimLock.ExitWriteLock(); }
//    }
//}
