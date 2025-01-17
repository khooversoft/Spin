using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Tools.Should;
using Toolbox.Types;
using Xunit.Abstractions;

namespace Toolbox.Graph.test.Stress;

public class CommandTwoNodesAndEdges : IWorker
{
    private readonly IGraphClient _graphClient;
    private readonly ITestOutputHelper _output;
    private readonly int _workNumber;

    public CommandTwoNodesAndEdges(IGraphClient graphClient, ITestOutputHelper output, int workNumber)
    {
        _graphClient = graphClient;
        _output = output;
        _workNumber = workNumber;
    }

    public Task<bool> Run(CancellationTokenSource token, ScopeContext context)
    {
        DateTime checkPoint = DateTime.UtcNow.AddSeconds(1);

        var ct = new TaskCompletionSource<bool>();

        _output.WriteLine($"Starting workNumber={_workNumber}");

        _ = Task.Run(async () =>
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    string tagKey = $"t1a-{_workNumber}";
                    string tag = $"t1a-{_workNumber}=v1";

                    var n1 = await AddNode(token, 1, tag, context);
                    var n2 = await AddNode(token, 2, tag, context);
                    var edge1 = await AddEdge(token, n1, n2, "default", context);

                    (await _graphClient.ExecuteBatch($"select (key={n1}) a1 -> [*] a2 ;", context)).Action(x =>
                    {
                        x.IsOk().Should().BeTrue();
                        x.Return().Action(y =>
                        {
                            y.Option.IsOk().Should().BeTrue();
                            y.Items.Count.Should().Be(2);
                            y.Items.Where(z => z.Alias == "a1").FirstOrDefault().Action(z1 =>
                            {
                                z1.Should().NotBeNull();
                                z1.NotNull().Nodes.Count.Should().Be(1);
                                z1.Nodes[0].Key.Should().Be(n1);
                                z1.Edges.Count.Should().Be(0);
                            });
                            y.Items.Where(z => z.Alias == "a2").FirstOrDefault().Action(z1 =>
                            {
                                z1.Should().NotBeNull();
                                z1.NotNull().Nodes.Count.Should().Be(0);
                                z1.Edges.Count.Should().Be(1);
                                (z1.Edges[0].GetPrimaryKey() == edge1).Should().BeTrue(edge1.ToString());
                            });
                        });
                    });

                    (await _graphClient.ExecuteBatch($"select [from={edge1.FromKey}, to={edge1.ToKey}, type={edge1.EdgeType} ] a1 <- (*) a2 ;", context)).Action(x =>
                    {
                        x.IsOk().Should().BeTrue();
                        x.Return().Action(y =>
                        {
                            y.Option.IsOk().Should().BeTrue();
                            y.Items.Count.Should().Be(2);
                            y.Items.Where(z => z.Alias == "a1").FirstOrDefault().Action(z1 =>
                            {
                                z1.Should().NotBeNull();
                                z1.NotNull().Nodes.Count.Should().Be(0);
                                z1.Edges.Count.Should().Be(1);
                                (z1.Edges[0].GetPrimaryKey() == edge1).Should().BeTrue(edge1.ToString());
                            });
                            y.Items.Where(z => z.Alias == "a2").FirstOrDefault().Action(z1 =>
                            {
                                z1.Should().NotBeNull();
                                z1.NotNull().Nodes.Count.Should().Be(1);
                                z1.Nodes[0].Key.Should().Be(n1);
                                z1.Edges.Count.Should().Be(0);
                            });
                        });
                    });


                    await Remove(token, n1, context);

                    await SelectNode(n1, 0, context);
                    await SelectNode(n2, 1, context);

                    (await _graphClient.Execute($"select [ from={edge1.FromKey}, to={edge1.ToKey}, type={edge1.EdgeType} ] ;", context)).Action(x =>
                    {
                        x.IsOk().Should().BeTrue();
                        x.Return().Action(y =>
                        {
                            y.Nodes.Count.Should().Be(0);
                            y.Edges.Count.Should().Be(0);
                        });
                    });

                    await Remove(token, n2, context);
                    await SelectNode(n2, 0, context);
                }
                catch (Exception ex)
                {
                    ct.SetException(ex);
                    _output.WriteLine($"Exception workNumber={_workNumber}, ex={ex}", ex.ToString());
                    return;
                }
            }

            _output.WriteLine($"Completed workNumber={_workNumber}");
            ct.SetResult(true);
        });

        return ct.Task;
    }

    private async Task SelectNode(string nodeKey, int expectedRowCount, ScopeContext context)
    {
        string cmd = $"select (key={nodeKey}) ;";
        var result = await _graphClient.Execute(cmd, context);
        result.IsOk().Should().BeTrue();
        var read = result.Return();

        read.Edges.Count.Should().Be(0);
        if (read.Nodes.Count == expectedRowCount) return;

        _output.WriteLine($"Expected row count={expectedRowCount}, actual={read.Nodes.Count}, rowkeys={read.Nodes.Select(x => x.Key).Join(',')}");
        read.Nodes.Count.Should().Be(expectedRowCount);
    }

    private async Task<string> AddNode(CancellationTokenSource token, int nodeId, string tag, ScopeContext context)
    {
        string nodeKey = $"nodeA-{_workNumber}-{nodeId}";

        string cmd = new NodeCommandBuilder()
            .SetNodeKey(nodeKey)
            .AddTag(tag)
            .Build();

        var result = await _graphClient.Execute(cmd, context);
        result.IsOk().Should().BeTrue();

        var selectCmd = new SelectCommandBuilder()
            .AddNodeSearch(x => x.SetNodeKey(nodeKey))
            .Build();

        var readOption = await _graphClient.Execute(selectCmd, context);
        readOption.IsOk().Should().BeTrue();
        QueryResult read = readOption.Return();
        read.Nodes.Count.Should().Be(1);
        read.Edges.Count.Should().Be(0);

        return nodeKey;
    }

    private async Task<GraphEdgePrimaryKey> AddEdge(CancellationTokenSource token, string fromNode, string toNode, string edgeType, ScopeContext context)
    {
        var cmd = new EdgeCommandBuilder()
            .SetFromKey(fromNode)
            .SetToKey(toNode)
            .SetEdgeType(edgeType)
            .Build();

        var result = await _graphClient.Execute(cmd, context);
        result.IsOk().Should().BeTrue();

        var selectCmd = new SelectCommandBuilder()
            .AddEdgeSearch(x => x.SetFromKey(fromNode).SetToKey(toNode).SetEdgeType(edgeType))
            .Build();

        var readOption = await _graphClient.Execute(selectCmd, context);
        readOption.IsOk().Should().BeTrue();
        QueryResult read = readOption.Return();
        read.Nodes.Count.Should().Be(0);
        read.Edges.Count.Should().Be(1);

        return read.Edges[0].GetPrimaryKey();
    }

    private async Task Remove(CancellationTokenSource token, string nodeKey, ScopeContext context)
    {
        string cmd = new DeleteCommandBuilder()
            .SetNodeKey(nodeKey)
            .Build();

        var result = await _graphClient.Execute(cmd, context);
        result.IsOk().Should().BeTrue();
    }
}
