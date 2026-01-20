//using Microsoft.Extensions.Logging;
//using Toolbox.Extensions;
//using Toolbox.Tools;
//using Toolbox.Types;

//namespace Toolbox.Graph.test.Stress;

//public class CommandThreeNodesAndEdges : IWorker
//{
//    private readonly IGraphClient _graphClient;
//    private readonly ILogger _logger;
//    private readonly int _workNumber;

//    public CommandThreeNodesAndEdges(IGraphClient graphClient, ILogger logger, int workNumber)
//    {
//        _graphClient = graphClient;
//        _logger = logger;
//        _workNumber = workNumber;
//    }

//    public Task<bool> Run(CancellationTokenSource token)
//    {
//        DateTime checkPoint = DateTime.UtcNow.AddSeconds(1);

//        var ct = new TaskCompletionSource<bool>();

//        _logger.LogInformation($"Starting workNumber={_workNumber}");

//        _ = Task.Run(async () =>
//        {
//            while (!token.IsCancellationRequested)
//            {
//                try
//                {
//                    string tagKey = $"t1d-{_workNumber}";
//                    string tag = $"t1d-{_workNumber}=v1";

//                    var n1 = await AddNode(token, 1, tag);
//                    var n2 = await AddNode(token, 2, tag);
//                    var n3 = await AddNode(token, 3, tag);
//                    var edge1 = await AddEdge(token, n1, n2, "default");
//                    var edge2 = await AddEdge(token, n2, n3, "default");

//                    await SelectRelationshipLeftJoin(n1);
//                    await SelectRelationshipLeftJoin(n2);
//                    await SelectRelationshipRightJoin(n2);
//                    await SelectEdgeRightJoin(edge1);
//                    await SelectEdge(edge1, 1);
//                    await SelectEdge(edge2, 1);

//                    await Remove(token, n1);

//                    await SelectNode(n1, 0);
//                    await SelectNode(n2, 1);
//                    await SelectNode(n3, 1);
//                    await SelectEdge(edge1, 0);
//                    await SelectEdge(edge2, 1);

//                    await Remove(token, n2);
//                    await SelectNode(n1, 0);
//                    await SelectNode(n2, 0);
//                    await SelectNode(n3, 1);
//                    await SelectEdge(edge1, 0);
//                    await SelectEdge(edge2, 0);

//                    await Remove(token, n3);
//                    await SelectNode(n1, 0);
//                    await SelectNode(n2, 0);
//                    await SelectNode(n3, 0);
//                }
//                catch (Exception ex)
//                {
//                    ct.SetException(ex);
//                    _logger.LogInformation($"Exception workNumber={_workNumber}, ex={ex}", ex.ToString());
//                    return;
//                }
//            }

//            _logger.LogInformation($"Completed workNumber={_workNumber}");
//            ct.SetResult(true);
//        });

//        return ct.Task;
//    }


//    private async Task<string> AddNode(CancellationTokenSource token, int nodeId, string tag)
//    {
//        string nodeKey = $"nodeD-{_workNumber}-{nodeId}";

//        string cmd = new NodeCommandBuilder()
//            .SetNodeKey(nodeKey)
//            .AddTag(tag)
//            .Build();

//        var result = await _graphClient.Execute(cmd);
//        result.IsOk().BeTrue(result.ToString());

//        var selectCmd = new SelectCommandBuilder()
//            .AddNodeSearch(x => x.SetNodeKey(nodeKey))
//            .Build();

//        var readOption = await _graphClient.Execute(selectCmd);
//        readOption.IsOk().BeTrue(readOption.ToString());
//        QueryResult read = readOption.Return();
//        read.Nodes.Count.Be(1);
//        read.Edges.Count.Be(0);

//        return nodeKey;
//    }

//    private async Task<GraphEdgePrimaryKey> AddEdge(CancellationTokenSource token, string fromNode, string toNode, string edgeType)
//    {
//        var cmd = new EdgeCommandBuilder()
//            .SetFromKey(fromNode)
//            .SetToKey(toNode)
//            .SetEdgeType(edgeType)
//            .Build();

//        var result = await _graphClient.Execute(cmd);
//        result.IsOk().BeTrue(result.ToString());

//        var selectCmd = new SelectCommandBuilder()
//            .AddEdgeSearch(x => x.SetFromKey(fromNode).SetToKey(toNode).SetEdgeType(edgeType))
//            .Build();

//        var readOption = await _graphClient.Execute(selectCmd);
//        readOption.IsOk().BeTrue(readOption.ToString());
//        QueryResult read = readOption.Return();
//        read.Nodes.Count.Be(0);
//        read.Edges.Count.Be(1);

//        return read.Edges[0].GetPrimaryKey();
//    }

//    private async Task Remove(CancellationTokenSource token, string nodeKey)
//    {
//        string cmd = new DeleteCommandBuilder()
//            .SetNodeKey(nodeKey)
//            .Build();

//        var result = await _graphClient.Execute(cmd);
//        result.IsOk().BeTrue(result.ToString());
//    }

//    private async Task SelectNode(string nodeKey, int expectedRowCount)
//    {
//        string cmd = $"select (key={nodeKey}) ;";
//        var result = await _graphClient.Execute(cmd);
//        result.IsOk().BeTrue(result.ToString());
//        var read = result.Return();

//        read.Edges.Count.Be(0);
//        if (read.Nodes.Count == expectedRowCount) return;

//        _logger.LogInformation($"Expected row count={expectedRowCount}, actual={read.Nodes.Count}, rowkeys={read.Nodes.Select(x => x.Key).Join(',')}");
//        read.Nodes.Count.Be(expectedRowCount);
//    }

//    private async Task SelectRelationshipLeftJoin(string nodeKey)
//    {
//        var result = await _graphClient.ExecuteBatch($"select (key={nodeKey}) a1 -> [*] a2 ;");
//        result.IsOk().BeTrue(result.ToString());
//        var read = result.Return();
//        read.Option.IsOk().BeTrue();

//        read.Items.Count.Be(2);
//        read.Items.Where(z => z.Alias == "a1").FirstOrDefault().Action(z1 =>
//        {
//            z1.NotNull();
//            z1.NotNull().Nodes.Count.Be(1);
//            z1.Edges.Count.Be(0);
//        });
//        read.Items.Where(z => z.Alias == "a2").FirstOrDefault().Action(z1 =>
//        {
//            z1.NotNull();
//            z1.NotNull().Nodes.Count.Be(0);
//            z1.Edges.Count.Be(1);
//        });
//    }

//    private async Task SelectRelationshipRightJoin(string nodeKey)
//    {
//        var result = await _graphClient.ExecuteBatch($"select (key={nodeKey}) a1 <- [*] a2 <- (*) a3 ;");
//        result.IsOk().BeTrue(result.ToString());
//        var read = result.Return();
//        read.Option.IsOk().BeTrue();

//        read.Items.Count.Be(3);
//        read.Items.Where(z => z.Alias == "a1").FirstOrDefault().Action(z1 =>
//        {
//            z1.NotNull();
//            z1.NotNull().Nodes.Count.Be(1);
//            z1.Edges.Count.Be(0);
//        });
//        read.Items.Where(z => z.Alias == "a2").FirstOrDefault().Action(z1 =>
//        {
//            z1.NotNull();
//            z1.NotNull().Nodes.Count.Be(0);
//            z1.Edges.Count.Be(1);
//        });
//        read.Items.Where(z => z.Alias == "a3").FirstOrDefault().Action(z1 =>
//        {
//            z1.NotNull();
//            z1.NotNull().Nodes.Count.Be(1);
//            z1.Edges.Count.Be(0);
//        });
//    }

//    private async Task SelectEdge(GraphEdgePrimaryKey edge1, int edgeCount)
//    {
//        var result = await _graphClient.Execute($"select [ from={edge1.FromKey}, to={edge1.ToKey}, type={edge1.EdgeType} ] ;");
//        result.IsOk().BeTrue(result.ToString());
//        var read = result.Return();

//        read.Nodes.Count.Be(0);
//        read.Edges.Count.Be(edgeCount);
//    }

//    private async Task SelectEdgeRightJoin(GraphEdgePrimaryKey edge1)
//    {
//        var result = await _graphClient.ExecuteBatch($"select [ from={edge1.FromKey}, to={edge1.ToKey}, type={edge1.EdgeType} ] a1 <- (*) a2 ;");
//        result.IsOk().BeTrue(result.ToString());
//        var read = result.Return();

//        read.Option.IsOk().BeTrue();
//        read.Items.Count.Be(2);
//        read.Items.Where(z => z.Alias == "a1").FirstOrDefault().Action(z1 =>
//        {
//            z1.NotNull();
//            z1.NotNull().Nodes.Count.Be(0);
//            z1.Edges.Count.Be(1);
//        });
//        read.Items.Where(z => z.Alias == "a2").FirstOrDefault().Action(z1 =>
//        {
//            z1.NotNull();
//            z1.NotNull().Nodes.Count.Be(1);
//            z1.Edges.Count.Be(0);
//        });
//    }
//}
