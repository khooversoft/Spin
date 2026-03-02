//using Microsoft.Extensions.Logging;
//using Toolbox.Data;
//using Toolbox.Telemetry;
//using Toolbox.Tools;
//using Toolbox.Types;

//namespace Toolbox.Graph;

//public class GrantGroupPrincipal
//{
//    private readonly Func<GraphCore?> _getGraph;
//    private readonly ReaderWriterLockSlim _slimLock;
//    private readonly ILogger _logger;
//    private readonly ITelemetry? _telemetry;

//    public GrantGroupPrincipal(Func<GraphCore?> getGraph, ReaderWriterLockSlim slimLock, ILogger logger, ITelemetry? telemetry)
//    {
//        _getGraph = getGraph;
//        _slimLock = slimLock;
//        _logger = logger;
//        _telemetry = telemetry;
//    }

//    public Option AddOrUpdate(string groupName, string principalId)
//    {
//        _slimLock.EnterWriteLock();
//        try 
//        {
//            var graph = _getGraph().NotNull("Graph not loaded");

//            var groupNodeKey = NodeTool.CreateKey(groupName, GrantGroup.NodeType);
//            if (!graph.Nodes.TryGetValue(groupNodeKey, out var groupNode)) return StatusCode.NotFound;

//            // Check if relathionship already exists
//            string principalNodeKey = NodeTool.CreateKey(principalId, PrincipalIdentity.NodeType);
//            var edgeKey = Edge.CreateKey(groupNodeKey, principalNodeKey, GrantGroupRegistry.EdgeType);
//            if (graph.Edges.Contains(edgeKey)) return StatusCode.OK;

//            // Add relationship
//            var edge = new Edge(groupNodeKey, principalNodeKey, GrantGroupRegistry.EdgeType);
//            var result = graph.Edges.Add(edge);
//            return result;
//        }
//        finally { _slimLock.ExitWriteLock(); }
//    }

//    public Option<IReadOnlyList<Node>> GetPrincipals(string groupName)
//    {
//        _slimLock.EnterReadLock();
//        try
//        {
//            var graph = _getGraph().NotNull("Graph not loaded");
//            var groupNodeKey = NodeTool.CreateKey(groupName, GrantGroup.NodeType);

//            var principalNodes = graph.Edges.GetByFrom(groupNodeKey)
//                .SelectMany(x => graph.Nodes.GetNodes(x.ToKey))
//                .ToArray();

//            return principalNodes!;
//        }
//        finally { _slimLock.ExitReadLock(); }
//    }

//    public Option<IReadOnlyList<Node>> GetGroups(string principalId)
//    {
//        _slimLock.EnterReadLock();
//        try
//        {
//            var graph = _getGraph().NotNull("Graph not loaded");
//            var groupNodeKey = NodeTool.CreateKey(principalId, PrincipalIdentity.NodeType);

//            var groupNodes = graph.Edges.GetByTo(groupNodeKey)
//                .SelectMany(x => graph.Nodes.GetNodes(x.FromKey))
//                .ToArray();

//            return groupNodes!;
//        }
//        finally { _slimLock.ExitReadLock(); }
//    }

//    public Option Remove(string groupName, string principalId)
//    {
//        _slimLock.EnterWriteLock();
//        try
//        {
//            var graph = _getGraph().NotNull("Graph not loaded");

//            // Remove relationship
//            var groupNodeKey = NodeTool.CreateKey(groupName, GrantGroup.NodeType);
//            string principalNodeKey = NodeTool.CreateKey(principalId, PrincipalIdentity.NodeType);
//            var edgeKey = Edge.CreateKey(groupNodeKey, principalNodeKey, GrantGroupRegistry.EdgeType);

//            var removeOption = graph.Edges.Remove(edgeKey);
//            return removeOption;
//        }
//        finally { _slimLock.ExitWriteLock(); }
//    }
//}
