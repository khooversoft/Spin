using FluentAssertions;
using Toolbox.Types;
using Xunit.Abstractions;

namespace Toolbox.Graph.test.Stress;

internal class WorkerTwoNodesAndEdge : IWorker
{
    private readonly GraphMap _map;
    private readonly ITestOutputHelper _output;
    private readonly int _workNumber;

    public WorkerTwoNodesAndEdge(GraphMap map, ITestOutputHelper output, int workNumber) => (_map, _output, _workNumber) = (map, output, workNumber);

    public Task<bool> Run(CancellationTokenSource token, ScopeContext context)
    {
        DateTime checkPoint = DateTime.UtcNow.AddSeconds(1);

        var ct = new TaskCompletionSource<bool>();

        _output.WriteLine($"Starting workNumber={_workNumber}");

        _ = Task.Run(() =>
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    string tagKey = $"t1a-{_workNumber}";
                    string tag = $"t1a-{_workNumber}=v1";

                    var n1 = AddNode(token, 1, tag);
                    var n2 = AddNode(token, 2, tag);
                    var edge = AddEdge(token, n1, n2, "default");

                    CheckState(token, () => _map.Nodes.LookupTaggedNodes(tagKey).Count == 2, $"lookup, tagKey={tagKey}, count={_map.Nodes.LookupTaggedNodes(tag).Count}");

                    CheckState(token, () => _map.Nodes.TryGetValue(n1, out var _) == true, n1 + " check");
                    CheckState(token, () => _map.Nodes.TryGetValue(n2, out var _) == true, n2 + " check");
                    CheckState(token, () => _map.Edges.TryGetValue(edge, out var _) == true, edge.ToString() + " check");

                    CheckState(token, () => _map.Nodes.Remove(n1).IsOk() == true, n1 + " removed");
                    CheckState(token, () => _map.Nodes.LookupTaggedNodes(tagKey).Count == 1, $"arter remove - t1 lookup, tagKey={tagKey}, count={_map.Nodes.LookupTaggedNodes("t1").Count}");

                    CheckState(token, () => _map.Nodes.TryGetValue(n1, out var _) == false, n1 + " after remove");
                    CheckState(token, () => _map.Nodes.TryGetValue(n2, out var _) == true, n2 + " after remove");
                    CheckState(token, () => _map.Edges.TryGetValue(edge, out var _) == false, edge.ToString() + " after remove");

                    CheckState(token, () => _map.Nodes.Remove(n2).IsOk(), n2 + "removed");
                    CheckState(token, () => _map.Nodes.LookupTaggedNodes(tagKey).Count == 0, $"after last remove - t1 lookup, tagKey={tagKey}, count={_map.Nodes.LookupTaggedNodes("t1").Count}");

                    CheckState(token, () => _map.Nodes.TryGetValue(n1, out var _) == false, n1 + " after last remove");
                    CheckState(token, () => _map.Nodes.TryGetValue(n2, out var _) == false, n2 + " after last remove");
                    CheckState(token, () => _map.Edges.TryGetValue(edge, out var _) == false, edge.ToString() + " after last remove");
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

    private string AddNode(CancellationTokenSource token, int nodeId, string tag)
    {
        string nodeKey = $"nodeA-{_workNumber}-{nodeId}";

        var node = new GraphNode(nodeKey, tag);
        _map.Add(node);
        //_output.WriteLine($"Added node {nodeKey}");

        GraphNode? readNode = null;
        CheckState(token, () => _map.Nodes.TryGetValue(nodeKey, out readNode) == true, nodeKey + " node check");
        (node == readNode).Should().BeTrue();

        return nodeKey;
    }

    private GraphEdgePrimaryKey AddEdge(CancellationTokenSource token, string fromNode, string toNode, string edgeType)
    {
        var edge = new GraphEdge(fromNode, toNode, edgeType);
        _map.Add(edge);
        //_output.WriteLine($"Added edge {edge}");

        var pk = edge.GetPrimaryKey();
        CheckState(token, () => _map.Edges.TryGetValue(pk, out var readEdge) == true, pk + " edge check");

        return pk;
    }

    private void CheckState(CancellationTokenSource token, Func<bool> func, string desc)
    {
        var result = func();
        if (result) return;

        token.Cancel();
        _output.WriteLine($"Canceled token, Node count {_map.Nodes.Count}, Edge count={_map.Edges.Count}, desc={desc}");

        foreach (var node in _map.Nodes.ToArray())
        {
            _output.WriteLine($"List - Node key={node.Key}");
        }

        foreach (var edge in _map.Edges.ToArray())
        {
            _output.WriteLine($"List - Edge from={edge.FromKey}, to={edge.ToKey}, type={edge.EdgeType}");
        }

        throw new InvalidOperationException(desc);
    }
}