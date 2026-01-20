//using Microsoft.Extensions.Logging;
//using Toolbox.Extensions;
//using Toolbox.Tools;
//using Toolbox.Types;

//namespace Toolbox.Graph.test.Stress;

//public class CommandTwoNodesAndEdges : IWorker
//{
//    private readonly IGraphClient _graphClient;
//    private readonly ILogger _logger;
//    private readonly int _workNumber;

//    public CommandTwoNodesAndEdges(IGraphClient graphClient, ILogger logger, int workNumber)
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
//                    string tagKey = $"t1a-{_workNumber}";
//                    string tag = $"t1a-{_workNumber}=v1";

//                    var n1 = await AddNode(token, 1, tag);
//                    var n2 = await AddNode(token, 2, tag);
//                    var edge1 = await AddEdge(token, n1, n2, "default");

//                    (await _graphClient.ExecuteBatch($"select (key={n1}) a1 -> [*] a2 ;")).Action(x =>
//                    {
//                        x.IsOk().BeTrue();
//                        x.Return().Action(y =>
//                        {
//                            y.Option.IsOk().BeTrue();
//                            y.Items.Count.Be(2);
//                            y.Items.Where(z => z.Alias == "a1").FirstOrDefault().Action(z1 =>
//                            {
//                                z1.NotNull();
//                                z1.NotNull().Nodes.Count.Be(1);
//                                z1.Nodes[0].Key.Be(n1);
//                                z1.Edges.Count.Be(0);
//                            });
//                            y.Items.Where(z => z.Alias == "a2").FirstOrDefault().Action(z1 =>
//                            {
//                                z1.NotNull();
//                                z1.NotNull().Nodes.Count.Be(0);
//                                z1.Edges.Count.Be(1);
//                                (z1.Edges[0].GetPrimaryKey() == edge1).BeTrue(edge1.ToString());
//                            });
//                        });
//                    });

//                    (await _graphClient.ExecuteBatch($"select [from={edge1.FromKey}, to={edge1.ToKey}, type={edge1.EdgeType} ] a1 <- (*) a2 ;")).Action(x =>
//                    {
//                        x.IsOk().BeTrue();
//                        x.Return().Action(y =>
//                        {
//                            y.Option.IsOk().BeTrue();
//                            y.Items.Count.Be(2);
//                            y.Items.Where(z => z.Alias == "a1").FirstOrDefault().Action(z1 =>
//                            {
//                                z1.NotNull();
//                                z1.NotNull().Nodes.Count.Be(0);
//                                z1.Edges.Count.Be(1);
//                                (z1.Edges[0].GetPrimaryKey() == edge1).BeTrue(edge1.ToString());
//                            });
//                            y.Items.Where(z => z.Alias == "a2").FirstOrDefault().Action(z1 =>
//                            {
//                                z1.NotNull();
//                                z1.NotNull().Nodes.Count.Be(1);
//                                z1.Nodes[0].Key.Be(n1);
//                                z1.Edges.Count.Be(0);
//                            });
//                        });
//                    });


//                    await Remove(token, n1);

//                    await SelectNode(n1, 0);
//                    await SelectNode(n2, 1);

//                    (await _graphClient.Execute($"select [ from={edge1.FromKey}, to={edge1.ToKey}, type={edge1.EdgeType} ] ;")).Action(x =>
//                    {
//                        x.IsOk().BeTrue();
//                        x.Return().Action(y =>
//                        {
//                            y.Nodes.Count.Be(0);
//                            y.Edges.Count.Be(0);
//                        });
//                    });

//                    await Remove(token, n2);
//                    await SelectNode(n2, 0);
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

//    private async Task SelectNode(string nodeKey, int expectedRowCount)
//    {
//        string cmd = $"select (key={nodeKey}) ;";
//        var result = await _graphClient.Execute(cmd);
//        result.IsOk().BeTrue();
//        var read = result.Return();

//        read.Edges.Count.Be(0);
//        if (read.Nodes.Count == expectedRowCount) return;

//        _logger.LogInformation($"Expected row count={expectedRowCount}, actual={read.Nodes.Count}, rowkeys={read.Nodes.Select(x => x.Key).Join(',')}");
//        read.Nodes.Count.Be(expectedRowCount);
//    }

//    private async Task<string> AddNode(CancellationTokenSource token, int nodeId, string tag)
//    {
//        string nodeKey = $"nodeA-{_workNumber}-{nodeId}";

//        string cmd = new NodeCommandBuilder()
//            .SetNodeKey(nodeKey)
//            .AddTag(tag)
//            .Build();

//        var result = await _graphClient.Execute(cmd);
//        result.IsOk().BeTrue();

//        var selectCmd = new SelectCommandBuilder()
//            .AddNodeSearch(x => x.SetNodeKey(nodeKey))
//            .Build();

//        var readOption = await _graphClient.Execute(selectCmd);
//        readOption.IsOk().BeTrue();
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
//        result.IsOk().BeTrue();

//        var selectCmd = new SelectCommandBuilder()
//            .AddEdgeSearch(x => x.SetFromKey(fromNode).SetToKey(toNode).SetEdgeType(edgeType))
//            .Build();

//        var readOption = await _graphClient.Execute(selectCmd);
//        readOption.IsOk().BeTrue();
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
//        result.IsOk().BeTrue();
//    }
//}
