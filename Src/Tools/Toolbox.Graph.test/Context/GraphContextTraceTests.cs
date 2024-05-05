//using System.Collections.Immutable;
//using FluentAssertions;
//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.Logging.Abstractions;
//using Toolbox.Extensions;
//using Toolbox.Graph;
//using Toolbox.Store;
//using Toolbox.Tools;
//using Toolbox.Types;

//namespace Toolbox.Graph.test.Context;

//public class GraphContextTraceTests
//{
//    private record DummyClass(string Name, int Age);

//    [Fact]
//    public async Task SimpleAddNode()
//    {
//        GraphTestClient graphClient = GraphTestStartup.CreateGraphTestHost();
//        GraphMap map = graphClient.ServiceProvider.GetRequiredService<GraphMap>();
//        InMemoryGraphFileStore store = (InMemoryGraphFileStore)graphClient.ServiceProvider.GetRequiredService<IGraphFileStore>();

//        var addResult = await graphClient.ExecuteScalar("add node key=node1;", NullScopeContext.Instance);
//        addResult.IsOk().Should().BeTrue();

//        map.Nodes.Count.Should().Be(1);
//        map.Edges.Count.Should().Be(0);
//        var n1 = map.Nodes.First();
//        n1.Key.Should().Be("node1");

//        store.Count.Should().Be(0);
//    }

//    [Fact]
//    public async Task SimpleAddNodeWithFile()
//    {
//        GraphTestClient graphClient = GraphTestStartup.CreateGraphTestHost();
//        GraphMap map = graphClient.ServiceProvider.GetRequiredService<GraphMap>();
//        InMemoryGraphFileStore store = (InMemoryGraphFileStore)graphClient.ServiceProvider.GetRequiredService<IGraphFileStore>();

//        var rec = new DummyClass("name1", 10);
//        string rec64 = rec.ToJson64();
//        var addResult = await graphClient.ExecuteScalar($"add node key=node1, lease {{ '{rec64}' }};", NullScopeContext.Instance);
//        addResult.IsOk().Should().BeTrue();

//        map.Nodes.Count.Should().Be(1);
//        map.Edges.Count.Should().Be(0);
//        var n1 = map.Nodes.First();
//        n1.Key.Should().Be("node1");

//        store.Count.Should().Be(1);
//        var storeRead = await store.Get("nodes/node1/node1___lease.json", NullScopeContext.Instance);
//        storeRead.IsOk().Should().BeTrue();

//        DataETag read = storeRead.Return();
//        var readRec = read.ToObject<DummyClass>();
//        readRec.Should().NotBeNull();
//        rec.Name.Should().Be(readRec.Name);
//        rec.Age.Should().Be(readRec.Age);
//    }


//    //[Fact]
//    //public async Task FullNodeLifeCycle()
//    //{
//    //    var map = new GraphMap();
//    //    var store = new InMemoryFileStore(NullLogger<InMemoryFileStore>.Instance);
//    //    var trace = new InMemoryChangeTrace();
//    //    var graphContext = new GraphContext(map, store, trace, NullScopeContext.Instance);

//    //    var addResult = await graphContext.ExecuteScalar("add node key=node1;");
//    //    addResult.IsOk().Should().BeTrue();

//    //    map.Nodes.Count.Should().Be(1);
//    //    map.Edges.Count.Should().Be(0);
//    //    store.Count.Should().Be(0);
//    //    trace.Count.Should().Be(1);

//    //    var traces = trace.GetTraces();
//    //    CheckCurrentNode(traces[0], ChangeTrxType.NodeAdd, "node1", "");

//    //    var updateResult = await graphContext.ExecuteScalar("update (key=node1) set t1=v1;");
//    //    updateResult.IsOk().Should().BeTrue();

//    //    map.Nodes.Count.Should().Be(1);
//    //    map.Edges.Count.Should().Be(0);
//    //    store.Count.Should().Be(0);
//    //    trace.Count.Should().Be(2);

//    //    traces = trace.GetTraces();
//    //    CheckCurrentNode(traces[0], ChangeTrxType.NodeAdd, "node1", "");
//    //    CheckCurrentAndUpdateNode(traces[1], ChangeTrxType.NodeChange, "node1", "", "t1=v1");

//    //    var deleteResult = await graphContext.ExecuteScalar("delete (key=node1);");
//    //    deleteResult.IsOk().Should().BeTrue();

//    //    map.Nodes.Count.Should().Be(0);
//    //    map.Edges.Count.Should().Be(0);
//    //    store.Count.Should().Be(0);
//    //    trace.Count.Should().Be(3);

//    //    traces = trace.GetTraces();
//    //    CheckCurrentNode(traces[0], ChangeTrxType.NodeAdd, "node1", "");
//    //    CheckCurrentAndUpdateNode(traces[1], ChangeTrxType.NodeChange, "node1", "", "t1=v1");

//    //    traces[2].ToObject<ChangeTrx>().NotNull().Action(x =>
//    //    {
//    //        x.TrxType.Should().Be(ChangeTrxType.NodeDelete);
//    //        x.CurrentNodeValue.NotNull().Action(y =>
//    //        {
//    //            y.Key.Should().Be("node1");
//    //            y.Tags.ToTagsString().Should().Be("t1=v1");
//    //        });
//    //        x.UpdateNodeValue.Should().BeNull();
//    //        x.CurrentEdgeValue.Should().BeNull();
//    //        x.UpdateEdgeValue.Should().BeNull();
//    //    });
//    //}

//    //[Fact]
//    //public async Task SimpleAddEdge()
//    //{
//    //    var map = new GraphMap();
//    //    var store = new InMemoryFileStore(NullLogger<InMemoryFileStore>.Instance);
//    //    var trace = new InMemoryChangeTrace();
//    //    var graphContext = new GraphContext(map, store, trace, NullScopeContext.Instance);

//    //    var cmd = """
//    //        add node key=node1;
//    //        add node key=node2;
//    //        add unique edge fromKey=node1, toKey=node2;
//    //        """;

//    //    var addResult = await graphContext.ExecuteScalar(cmd);
//    //    addResult.IsOk().Should().BeTrue();

//    //    map.Nodes.Count.Should().Be(2);
//    //    map.Edges.Count.Should().Be(1);
//    //    store.Count.Should().Be(0);
//    //    trace.Count.Should().Be(3);

//    //    var traces = trace.GetTraces();
//    //    traces.Count.Should().Be(3);
//    //    CheckCurrentNode(traces[0], ChangeTrxType.NodeAdd, "node1", "");
//    //    CheckCurrentNode(traces[1], ChangeTrxType.NodeAdd, "node2", "");
//    //    CheckCurrentEdge(traces[2], ChangeTrxType.EdgeAdd, "node1", "node2", "");
//    //}

//    //[Fact]
//    //public async Task AddEdgeFullLifecycle()
//    //{
//    //    var map = new GraphMap();
//    //    var store = new InMemoryFileStore(NullLogger<InMemoryFileStore>.Instance);
//    //    var trace = new InMemoryChangeTrace();
//    //    var graphContext = new GraphContext(map, store, trace, NullScopeContext.Instance);

//    //    var cmd = """
//    //        add node key=node1;
//    //        add node key=node2;
//    //        add unique edge fromKey=node1, toKey=node2;
//    //        """;

//    //    var addResult = await graphContext.ExecuteScalar(cmd);
//    //    addResult.IsOk().Should().BeTrue();

//    //    map.Nodes.Count.Should().Be(2);
//    //    map.Edges.Count.Should().Be(1);
//    //    store.Count.Should().Be(0);
//    //    trace.Count.Should().Be(3);

//    //    var traces = trace.GetTraces();
//    //    traces.Count.Should().Be(3);
//    //    CheckCurrentNode(traces[0], ChangeTrxType.NodeAdd, "node1", "");
//    //    CheckCurrentNode(traces[1], ChangeTrxType.NodeAdd, "node2", "");
//    //    CheckCurrentEdge(traces[2], ChangeTrxType.EdgeAdd, "node1", "node2", "");

//    //    var updateResult = await graphContext.ExecuteScalar("update [fromKey=node1, toKey=node2] set t1=v1;");
//    //    updateResult.IsOk().Should().BeTrue();
//    //    updateResult.Return().Items.Length.Should().Be(1);

//    //    traces = trace.GetTraces();
//    //    traces.Count.Should().Be(4);
//    //    CheckCurrentNode(traces[0], ChangeTrxType.NodeAdd, "node1", "");
//    //    CheckCurrentNode(traces[1], ChangeTrxType.NodeAdd, "node2", "");
//    //    CheckCurrentEdge(traces[2], ChangeTrxType.EdgeAdd, "node1", "node2", "");
//    //    CheckCurrentAndUpdateEdge(traces[3], ChangeTrxType.EdgeChange, "node1", "node2", "", "t1=v1");

//    //    var deleteResult = await graphContext.ExecuteScalar("delete [fromKey=node1, toKey=node2];");
//    //    deleteResult.IsOk().Should().BeTrue();
//    //    deleteResult.Return().Items.Length.Should().Be(1);

//    //    traces = trace.GetTraces();
//    //    traces.Count.Should().Be(5);
//    //    CheckCurrentNode(traces[0], ChangeTrxType.NodeAdd, "node1", "");
//    //    CheckCurrentNode(traces[1], ChangeTrxType.NodeAdd, "node2", "");
//    //    CheckCurrentEdge(traces[2], ChangeTrxType.EdgeAdd, "node1", "node2", "");
//    //    CheckCurrentAndUpdateEdge(traces[3], ChangeTrxType.EdgeChange, "node1", "node2", "", "t1=v1");

//    //    traces[4].ToObject<ChangeTrx>().NotNull().Action(x =>
//    //    {
//    //        x.TrxType.Should().Be(ChangeTrxType.EdgeDelete);
//    //        x.CurrentNodeValue.Should().BeNull();
//    //        x.UpdateNodeValue.Should().BeNull();
//    //        x.CurrentEdgeValue.NotNull().Action(y =>
//    //        {
//    //            y.FromKey.Should().Be("node1");
//    //            y.ToKey.Should().Be("node2");
//    //            y.Tags.ToTagsString().Should().Be("t1=v1");
//    //        });
//    //        x.UpdateEdgeValue.Should().BeNull();
//    //    });
//    //}

//    //private static void CheckCurrentNode(string data, ChangeTrxType trxType, string nodeKey, string tags)
//    //{
//    //    ChangeTrx trx = data.ToObject<ChangeTrx>().NotNull();
//    //    trx.TrxType.Should().Be(trxType);
//    //    trx.CurrentNodeValue.NotNull().Action(y =>
//    //    {
//    //        y.Key.Should().Be(nodeKey);
//    //        y.Tags.ToTagsString().Should().Be(tags);
//    //    });
//    //    trx.UpdateNodeValue.Should().BeNull();
//    //    trx.CurrentEdgeValue.Should().BeNull();
//    //    trx.UpdateEdgeValue.Should().BeNull();
//    //}

//    //private static void CheckCurrentAndUpdateNode(string data, ChangeTrxType trxType, string nodeKey, string tags, string updatedTags)
//    //{
//    //    ChangeTrx trx = data.ToObject<ChangeTrx>().NotNull();
//    //    trx.TrxType.Should().Be(trxType);
//    //    trx.CurrentNodeValue.NotNull().Action(y =>
//    //    {
//    //        y.Key.Should().Be(nodeKey);
//    //        y.Tags.ToTagsString().Should().Be(tags);
//    //    });
//    //    trx.UpdateNodeValue.NotNull().Action(y =>
//    //    {
//    //        y.Key.Should().Be(nodeKey);
//    //        y.Tags.ToTagsString().Should().Be(updatedTags);
//    //    });
//    //    trx.CurrentEdgeValue.Should().BeNull();
//    //    trx.UpdateEdgeValue.Should().BeNull();
//    //}

//    //private static void CheckCurrentEdge(string data, ChangeTrxType trxType, string fromNode, string toNode, string tags)
//    //{
//    //    ChangeTrx trx = data.ToObject<ChangeTrx>().NotNull();
//    //    trx.TrxType.Should().Be(trxType);
//    //    trx.CurrentNodeValue.Should().BeNull();
//    //    trx.UpdateNodeValue.Should().BeNull();
//    //    trx.CurrentEdgeValue.NotNull().Action(y =>
//    //    {
//    //        y.FromKey.Should().Be(fromNode);
//    //        y.ToKey.Should().Be(toNode);
//    //        y.Tags.ToTagsString().Should().Be(tags);
//    //    });
//    //    trx.UpdateEdgeValue.Should().BeNull();
//    //}

//    //private static void CheckCurrentAndUpdateEdge(string data, ChangeTrxType trxType, string fromNode, string toNode, string tags, string updatedTags)
//    //{
//    //    ChangeTrx trx = data.ToObject<ChangeTrx>().NotNull();
//    //    trx.TrxType.Should().Be(trxType);
//    //    trx.CurrentNodeValue.Should().BeNull();
//    //    trx.UpdateNodeValue.Should().BeNull();
//    //    trx.CurrentEdgeValue.NotNull().Action(y =>
//    //    {
//    //        y.FromKey.Should().Be(fromNode);
//    //        y.ToKey.Should().Be(toNode);
//    //        y.Tags.ToTagsString().Should().Be(tags);
//    //    });
//    //    trx.UpdateEdgeValue.NotNull().Action(y =>
//    //    {
//    //        y.FromKey.Should().Be(fromNode);
//    //        y.ToKey.Should().Be(toNode);
//    //        y.Tags.ToTagsString().Should().Be(updatedTags);
//    //    });
//    //}
//}
