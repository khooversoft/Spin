//using System.Runtime.CompilerServices;
//using Toolbox.Tools;
//using Toolbox.Types;
//using Xunit.Abstractions;

//namespace Toolbox.Graph.test.Stress;

//internal class WorkerThreeNodesAndEdges : IWorker
//{
//    private readonly GraphMap _map;
//    private readonly ITestOutputHelper _output;
//    private readonly int _workNumber;

//    public WorkerThreeNodesAndEdges(GraphMap map, ITestOutputHelper output, int workNumber) => (_map, _output, _workNumber) = (map, output, workNumber);

//    public Task<bool> Run(CancellationTokenSource token)
//    {
//        DateTime checkPoint = DateTime.UtcNow.AddSeconds(1);

//        var ct = new TaskCompletionSource<bool>();

//        _output.WriteLine($"Starting workNumber={_workNumber}");

//        _ = Task.Run(() =>
//        {
//            while (!token.IsCancellationRequested)
//            {
//                try
//                {
//                    string tagKey = $"t1B-{_workNumber}";
//                    string tag = $"t1B-{_workNumber}=v1";

//                    var n1 = AddNode(token, 1, tag);
//                    var n2 = AddNode(token, 2, tag);
//                    var n3 = AddNode(token, 3, tag);
//                    var edge1 = AddEdge(token, n1, n2, "default");
//                    var edge2 = AddEdge(token, n2, n3, "default");
//                    var edge3 = AddEdge(token, n3, n1, "default");

//                    CheckState(token, () => _map.Nodes.LookupTaggedNodes(tagKey).Count == 3, $"lookup, tagKey={tagKey}, count={_map.Nodes.LookupTaggedNodes(tag).Count}");

//                    CheckState(token, () => _map.Nodes.TryGetValue(n1, out var _) == true, n1 + " init");
//                    CheckState(token, () => _map.Nodes.TryGetValue(n2, out var _) == true, n2 + " init");
//                    CheckState(token, () => _map.Nodes.TryGetValue(n3, out var _) == true, n3 + " init");
//                    CheckState(token, () => _map.Edges.TryGetValue(edge1, out var _) == true, edge1.ToString() + " init");
//                    CheckState(token, () => _map.Edges.TryGetValue(edge2, out var _) == true, edge2.ToString() + " init");
//                    CheckState(token, () => _map.Edges.TryGetValue(edge3, out var _) == true, edge3.ToString() + " init");

//                    CheckState(token, () => _map.Nodes.Remove(n1).IsOk() == true, n1 + " removed");
//                    CheckState(token, () => _map.Nodes.LookupTaggedNodes(tagKey).Count == 2, $"remove 1, tagKey={tagKey}, count={_map.Nodes.LookupTaggedNodes(tag).Count}");

//                    CheckState(token, () => _map.Nodes.TryGetValue(n1, out var _) == false, n1 + " 1st remove");
//                    CheckState(token, () => _map.Nodes.TryGetValue(n2, out var _) == true, n2 + " 1st remove");
//                    CheckState(token, () => _map.Nodes.TryGetValue(n3, out var _) == true, n3 + " 1st remove");
//                    CheckState(token, () => _map.Edges.TryGetValue(edge1, out var _) == false, edge1.ToString() + " 1st remove");
//                    CheckState(token, () => _map.Edges.TryGetValue(edge2, out var _) == true, edge2.ToString() + " 1st remove");
//                    CheckState(token, () => _map.Edges.TryGetValue(edge3, out var _) == false, edge3.ToString() + " 1st remove");

//                    CheckState(token, () => _map.Nodes.Remove(n2).IsOk(), n2 + "removed");
//                    CheckState(token, () => _map.Nodes.LookupTaggedNodes(tagKey).Count == 1, $"remove 2, tagKey={tagKey}, count={_map.Nodes.LookupTaggedNodes(tag).Count}");

//                    CheckState(token, () => _map.Nodes.TryGetValue(n1, out var _) == false, n1 + " 2st remove");
//                    CheckState(token, () => _map.Nodes.TryGetValue(n2, out var _) == false, n2 + " 2st remove");
//                    CheckState(token, () => _map.Nodes.TryGetValue(n3, out var _) == true, n3 + " 2st remove");
//                    CheckState(token, () => _map.Edges.TryGetValue(edge1, out var _) == false, edge1.ToString() + " 2st remove");
//                    CheckState(token, () => _map.Edges.TryGetValue(edge2, out var _) == false, edge2.ToString() + " 2st remove");
//                    CheckState(token, () => _map.Edges.TryGetValue(edge3, out var _) == false, edge3.ToString() + " 2st remove");

//                    CheckState(token, () => _map.Nodes.Remove(n3).IsOk(), n3 + "removed");
//                    CheckState(token, () => _map.Nodes.LookupTaggedNodes(tagKey).Count == 0, $"remove 3, tagKey={tagKey}, count={_map.Nodes.LookupTaggedNodes(tag).Count}");

//                    CheckState(token, () => _map.Nodes.TryGetValue(n1, out var _) == false, n1 + " 3st remove");
//                    CheckState(token, () => _map.Nodes.TryGetValue(n2, out var _) == false, n2 + " 3st remove");
//                    CheckState(token, () => _map.Nodes.TryGetValue(n3, out var _) == false, n3 + " 3st remove");
//                    CheckState(token, () => _map.Edges.TryGetValue(edge1, out var _) == false, edge1.ToString() + " 3st remove");
//                    CheckState(token, () => _map.Edges.TryGetValue(edge2, out var _) == false, edge2.ToString() + " 3st remove");
//                    CheckState(token, () => _map.Edges.TryGetValue(edge3, out var _) == false, edge3.ToString() + " 3st remove");
//                }
//                catch (Exception ex)
//                {
//                    ct.SetException(ex);
//                    _output.WriteLine($"Exception workNumber={_workNumber}, ex={ex}", ex.ToString());
//                    return;
//                }
//            }

//            _output.WriteLine($"Completed workNumber={_workNumber}");
//            ct.SetResult(true);
//        });

//        return ct.Task;
//    }

//    private string AddNode(CancellationTokenSource token, int nodeId, string tag)
//    {
//        string nodeKey = $"nodeB-{_workNumber}-{nodeId}";

//        var node = new GraphNode(nodeKey, tag);
//        _map.Add(node);
//        //_output.WriteLine($"Added node {nodeKey}");

//        GraphNode? readNode = null;
//        CheckState(token, () => _map.Nodes.TryGetValue(nodeKey, out readNode) == true, nodeKey + " node check");
//        (node == readNode).BeTrue();

//        //var lookupTag = _map.Nodes.LookupTag("t1");
//        //lookupTag.Count.BeGreaterThan(0);
//        //lookupTag.Contains(nodeKey).BeTrue();

//        return nodeKey;
//    }

//    private GraphEdgePrimaryKey AddEdge(CancellationTokenSource token, string fromNode, string toNode, string edgeType)
//    {
//        var edge = new GraphEdge(fromNode, toNode, edgeType);
//        _map.Add(edge);
//        //_output.WriteLine($"Added edge {edge}");

//        var pk = edge.GetPrimaryKey();
//        CheckState(token, () => _map.Edges.TryGetValue(pk, out var readEdge) == true, pk + " edge check");

//        return pk;
//    }

//    private void CheckState(
//            CancellationTokenSource token,
//            Func<bool> func, string desc,
//            [CallerLineNumber] int lineNumber = 0,
//            [CallerArgumentExpression("func")] string name = ""
//        )
//    {
//        var result = func();
//        if (result) return;

//        token.Cancel();
//        _output.WriteLine($"Canceled token, Node count {_map.Nodes.Count}, Edge count={_map.Edges.Count}, desc={desc}, workNumber={_workNumber}, lineNumber={lineNumber}, name={name}");

//        foreach (var node in _map.Nodes.ToArray())
//        {
//            _output.WriteLine($"List - Node key={node.Key}, workNumber={_workNumber}");
//        }

//        foreach (var edge in _map.Edges.ToArray())
//        {
//            _output.WriteLine($"List - Edge from={edge.FromKey}, to={edge.ToKey}, type={edge.EdgeType}, workNumber={_workNumber}");
//        }

//        throw new InvalidOperationException(desc);
//    }
//}
