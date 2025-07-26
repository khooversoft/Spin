using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;
using Xunit.Abstractions;

namespace Toolbox.Graph.test.Stress;

public class CommandThreeNodesAndEdges : IWorker
{
    private readonly IGraphClient _graphClient;
    private readonly ScopeContext _context;
    private readonly int _workNumber;

    public CommandThreeNodesAndEdges(IGraphClient graphClient, ScopeContext context, int workNumber)
    {
        _graphClient = graphClient;
        _context = context;
        _workNumber = workNumber;
    }

    public Task<bool> Run(CancellationTokenSource token, ScopeContext context)
    {
        DateTime checkPoint = DateTime.UtcNow.AddSeconds(1);

        var ct = new TaskCompletionSource<bool>();

        _context.LogInformation($"Starting workNumber={_workNumber}");

        _ = Task.Run(async () =>
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    string tagKey = $"t1d-{_workNumber}";
                    string tag = $"t1d-{_workNumber}=v1";

                    var n1 = await AddNode(token, 1, tag, context);
                    var n2 = await AddNode(token, 2, tag, context);
                    var n3 = await AddNode(token, 3, tag, context);
                    var edge1 = await AddEdge(token, n1, n2, "default", context);
                    var edge2 = await AddEdge(token, n2, n3, "default", context);

                    await SelectRelationshipLeftJoin(n1, context);
                    await SelectRelationshipLeftJoin(n2, context);
                    await SelectRelationshipRightJoin(n2, context);
                    await SelectEdgeRightJoin(edge1, context);
                    await SelectEdge(edge1, 1, context);
                    await SelectEdge(edge2, 1, context);

                    await Remove(token, n1, context);

                    await SelectNode(n1, 0, context);
                    await SelectNode(n2, 1, context);
                    await SelectNode(n3, 1, context);
                    await SelectEdge(edge1, 0, context);
                    await SelectEdge(edge2, 1, context);

                    await Remove(token, n2, context);
                    await SelectNode(n1, 0, context);
                    await SelectNode(n2, 0, context);
                    await SelectNode(n3, 1, context);
                    await SelectEdge(edge1, 0, context);
                    await SelectEdge(edge2, 0, context);

                    await Remove(token, n3, context);
                    await SelectNode(n1, 0, context);
                    await SelectNode(n2, 0, context);
                    await SelectNode(n3, 0, context);
                }
                catch (Exception ex)
                {
                    ct.SetException(ex);
                    _context.LogInformation($"Exception workNumber={_workNumber}, ex={ex}", ex.ToString());
                    return;
                }
            }

            _context.LogInformation($"Completed workNumber={_workNumber}");
            ct.SetResult(true);
        });

        return ct.Task;
    }


    private async Task<string> AddNode(CancellationTokenSource token, int nodeId, string tag, ScopeContext context)
    {
        string nodeKey = $"nodeD-{_workNumber}-{nodeId}";

        string cmd = new NodeCommandBuilder()
            .SetNodeKey(nodeKey)
            .AddTag(tag)
            .Build();

        var result = await _graphClient.Execute(cmd, context);
        result.IsOk().BeTrue(result.ToString());

        var selectCmd = new SelectCommandBuilder()
            .AddNodeSearch(x => x.SetNodeKey(nodeKey))
            .Build();

        var readOption = await _graphClient.Execute(selectCmd, context);
        readOption.IsOk().BeTrue(readOption.ToString());
        QueryResult read = readOption.Return();
        read.Nodes.Count.Be(1);
        read.Edges.Count.Be(0);

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
        result.IsOk().BeTrue(result.ToString());

        var selectCmd = new SelectCommandBuilder()
            .AddEdgeSearch(x => x.SetFromKey(fromNode).SetToKey(toNode).SetEdgeType(edgeType))
            .Build();

        var readOption = await _graphClient.Execute(selectCmd, context);
        readOption.IsOk().BeTrue(readOption.ToString());
        QueryResult read = readOption.Return();
        read.Nodes.Count.Be(0);
        read.Edges.Count.Be(1);

        return read.Edges[0].GetPrimaryKey();
    }

    private async Task Remove(CancellationTokenSource token, string nodeKey, ScopeContext context)
    {
        string cmd = new DeleteCommandBuilder()
            .SetNodeKey(nodeKey)
            .Build();

        var result = await _graphClient.Execute(cmd, context);
        result.IsOk().BeTrue(result.ToString());
    }

    private async Task SelectNode(string nodeKey, int expectedRowCount, ScopeContext context)
    {
        string cmd = $"select (key={nodeKey}) ;";
        var result = await _graphClient.Execute(cmd, context);
        result.IsOk().BeTrue(result.ToString());
        var read = result.Return();

        read.Edges.Count.Be(0);
        if (read.Nodes.Count == expectedRowCount) return;

        _context.LogInformation($"Expected row count={expectedRowCount}, actual={read.Nodes.Count}, rowkeys={read.Nodes.Select(x => x.Key).Join(',')}");
        read.Nodes.Count.Be(expectedRowCount);
    }

    private async Task SelectRelationshipLeftJoin(string nodeKey, ScopeContext context)
    {
        var result = await _graphClient.ExecuteBatch($"select (key={nodeKey}) a1 -> [*] a2 ;", context);
        result.IsOk().BeTrue(result.ToString());
        var read = result.Return();
        read.Option.IsOk().BeTrue();

        read.Items.Count.Be(2);
        read.Items.Where(z => z.Alias == "a1").FirstOrDefault().Action(z1 =>
        {
            z1.NotNull();
            z1.NotNull().Nodes.Count.Be(1);
            z1.Edges.Count.Be(0);
        });
        read.Items.Where(z => z.Alias == "a2").FirstOrDefault().Action(z1 =>
        {
            z1.NotNull();
            z1.NotNull().Nodes.Count.Be(0);
            z1.Edges.Count.Be(1);
        });
    }

    private async Task SelectRelationshipRightJoin(string nodeKey, ScopeContext context)
    {
        var result = await _graphClient.ExecuteBatch($"select (key={nodeKey}) a1 <- [*] a2 <- (*) a3 ;", context);
        result.IsOk().BeTrue(result.ToString());
        var read = result.Return();
        read.Option.IsOk().BeTrue();

        read.Items.Count.Be(3);
        read.Items.Where(z => z.Alias == "a1").FirstOrDefault().Action(z1 =>
        {
            z1.NotNull();
            z1.NotNull().Nodes.Count.Be(1);
            z1.Edges.Count.Be(0);
        });
        read.Items.Where(z => z.Alias == "a2").FirstOrDefault().Action(z1 =>
        {
            z1.NotNull();
            z1.NotNull().Nodes.Count.Be(0);
            z1.Edges.Count.Be(1);
        });
        read.Items.Where(z => z.Alias == "a3").FirstOrDefault().Action(z1 =>
        {
            z1.NotNull();
            z1.NotNull().Nodes.Count.Be(1);
            z1.Edges.Count.Be(0);
        });
    }

    private async Task SelectEdge(GraphEdgePrimaryKey edge1, int edgeCount, ScopeContext context)
    {
        var result = await _graphClient.Execute($"select [ from={edge1.FromKey}, to={edge1.ToKey}, type={edge1.EdgeType} ] ;", context);
        result.IsOk().BeTrue(result.ToString());
        var read = result.Return();

        read.Nodes.Count.Be(0);
        read.Edges.Count.Be(edgeCount);
    }

    private async Task SelectEdgeRightJoin(GraphEdgePrimaryKey edge1, ScopeContext context)
    {
        var result = await _graphClient.ExecuteBatch($"select [ from={edge1.FromKey}, to={edge1.ToKey}, type={edge1.EdgeType} ] a1 <- (*) a2 ;", context);
        result.IsOk().BeTrue(result.ToString());
        var read = result.Return();

        read.Option.IsOk().BeTrue();
        read.Items.Count.Be(2);
        read.Items.Where(z => z.Alias == "a1").FirstOrDefault().Action(z1 =>
        {
            z1.NotNull();
            z1.NotNull().Nodes.Count.Be(0);
            z1.Edges.Count.Be(1);
        });
        read.Items.Where(z => z.Alias == "a2").FirstOrDefault().Action(z1 =>
        {
            z1.NotNull();
            z1.NotNull().Nodes.Count.Be(1);
            z1.Edges.Count.Be(0);
        });
    }
}
