using System.Collections.Immutable;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Toolbox.Extensions;
using Toolbox.Graph;
using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph.test.Context;

public class GraphContextTraceTests
{
    [Fact]
    public void ChangeTrxSerialization()
    {
        var graphNode = new GraphNode("node1", "tag1=v1".ToTags(), DateTime.Now, ["link1", "link2"], ImmutableDictionary<string, GraphDataLink>.Empty);
        var graphData = graphNode.ToJson();
        GraphNode readGraphNode = graphData.ToObject<GraphNode>().NotNull();
        (graphNode == readGraphNode).Should().BeTrue();

        var trx = new ChangeTrx(ChangeTrxType.NodeAdd, Guid.NewGuid(), Guid.NewGuid(), new GraphNode("key1"), null);
        var data = trx.ToJson();
        ChangeTrx readTrx = data.ToObject<ChangeTrx>().NotNull();
        trx.Equals(readTrx).Should().BeTrue();
    }

    [Fact]
    public async Task SimpleAddNode()
    {
        var map = new GraphMap();
        var store = new InMemoryFileStore(NullLogger<InMemoryFileStore>.Instance);
        var trace = new InMemoryChangeTrace();
        var graphContext = new GraphContext(map, store, trace, NullScopeContext.Instance);

        var addResult = await graphContext.ExecuteScalar("add node key=node1;");
        addResult.IsOk().Should().BeTrue();

        map.Nodes.Count.Should().Be(1);
        map.Edges.Count.Should().Be(0);
        var n1 = map.Nodes.First();
        n1.Key.Should().Be("node1");

        store.Count.Should().Be(0);
        trace.Count.Should().Be(1);

        string data = trace.GetTraces().First();
        ChangeTrx trx = data.ToObject<ChangeTrx>().NotNull();
        var compareTo = new ChangeTrx(ChangeTrxType.NodeAdd, graphContext.ChangeLog.TrxId, trx.LogKey, n1, null, trx.Date);
        trx.Equals(compareTo).Should().BeTrue();

        trx.TrxType.Should().Be(ChangeTrxType.NodeAdd);
        trx.TrxId.Should().Be(graphContext.ChangeLog.TrxId);
        trx.CurrentNodeValue.NotNull().Key.Should().Be("node1");
        trx.UpdateNodeValue.Should().BeNull();
        trx.CurrentEdgeValue.Should().BeNull();
        trx.UpdateEdgeValue.Should().BeNull();
    }

    [Fact]
    public async Task SimpleAddNodeWithFile()
    {
        var map = new GraphMap();
        var store = new InMemoryFileStore(NullLogger<InMemoryFileStore>.Instance);
        var trace = new InMemoryChangeTrace();
        var shimmedFileStore = new FileStoreTraceShim(store, trace);
        var graphContext = new GraphContext(map, shimmedFileStore, trace, NullScopeContext.Instance);

        var addResult = await graphContext.ExecuteScalar("add node key=node1;");
        addResult.IsOk().Should().BeTrue();

        map.Nodes.Count.Should().Be(1);
        map.Edges.Count.Should().Be(0);
        var n1 = map.Nodes.First();
        n1.Key.Should().Be("node1");

        store.Count.Should().Be(0);
        trace.Count.Should().Be(1);

        var rec = new DummyClass("name1", 10);
        const string fileName = "nodes/data/DummyClass.json";
        var writeResult = await graphContext.Store!.Add(fileName, rec, NullScopeContext.Instance);
        writeResult.IsOk().Should().BeTrue();

        trace.Count.Should().Be(2);

        var traces = trace.GetTraces();
        traces[0].ToObject<ChangeTrx>().NotNull().Action(x =>
        {
            x.TrxType.Should().Be(ChangeTrxType.NodeAdd);
            x.CurrentNodeValue.NotNull().Action(y =>
            {
                y.Key.Should().Be("node1");
                y.Tags.Should().BeEmpty();
            });
            x.UpdateNodeValue.Should().BeNull();
            x.CurrentEdgeValue.Should().BeNull();
            x.UpdateEdgeValue.Should().BeNull();
        });
        traces[1].ToObject<ChangeTrx>().NotNull().Action(x =>
        {
            x.TrxType.Should().Be(ChangeTrxType.FileAdd);
            x.FilePath.Should().Be(fileName);

            var dataTag = rec.ToDataETag();
            (x.FileData == dataTag).Should().BeTrue();
        });

    }

    private record DummyClass(string Name, int Age);

    [Fact]
    public async Task FullNodeLifeCycle()
    {
        var map = new GraphMap();
        var store = new InMemoryFileStore(NullLogger<InMemoryFileStore>.Instance);
        var trace = new InMemoryChangeTrace();
        var graphContext = new GraphContext(map, store, trace, NullScopeContext.Instance);

        var addResult = await graphContext.ExecuteScalar("add node key=node1;");
        addResult.IsOk().Should().BeTrue();

        map.Nodes.Count.Should().Be(1);
        map.Edges.Count.Should().Be(0);
        store.Count.Should().Be(0);
        trace.Count.Should().Be(1);

        var traces = trace.GetTraces();
        CheckCurrentNode(traces[0], ChangeTrxType.NodeAdd, "node1", "");

        var updateResult = await graphContext.ExecuteScalar("update (key=node1) set t1=v1;");
        updateResult.IsOk().Should().BeTrue();

        map.Nodes.Count.Should().Be(1);
        map.Edges.Count.Should().Be(0);
        store.Count.Should().Be(0);
        trace.Count.Should().Be(2);

        traces = trace.GetTraces();
        CheckCurrentNode(traces[0], ChangeTrxType.NodeAdd, "node1", "");
        CheckCurrentAndUpdateNode(traces[1], ChangeTrxType.NodeChange, "node1", "", "t1=v1");

        var deleteResult = await graphContext.ExecuteScalar("delete (key=node1);");
        deleteResult.IsOk().Should().BeTrue();

        map.Nodes.Count.Should().Be(0);
        map.Edges.Count.Should().Be(0);
        store.Count.Should().Be(0);
        trace.Count.Should().Be(3);

        traces = trace.GetTraces();
        CheckCurrentNode(traces[0], ChangeTrxType.NodeAdd, "node1", "");
        CheckCurrentAndUpdateNode(traces[1], ChangeTrxType.NodeChange, "node1", "", "t1=v1");

        traces[2].ToObject<ChangeTrx>().NotNull().Action(x =>
        {
            x.TrxType.Should().Be(ChangeTrxType.NodeDelete);
            x.CurrentNodeValue.NotNull().Action(y =>
            {
                y.Key.Should().Be("node1");
                y.Tags.ToTagsString().Should().Be("t1=v1");
            });
            x.UpdateNodeValue.Should().BeNull();
            x.CurrentEdgeValue.Should().BeNull();
            x.UpdateEdgeValue.Should().BeNull();
        });
    }

    [Fact]
    public async Task SimpleAddEdge()
    {
        var map = new GraphMap();
        var store = new InMemoryFileStore(NullLogger<InMemoryFileStore>.Instance);
        var trace = new InMemoryChangeTrace();
        var graphContext = new GraphContext(map, store, trace, NullScopeContext.Instance);

        var cmd = """
            add node key=node1;
            add node key=node2;
            add unique edge fromKey=node1, toKey=node2;
            """;

        var addResult = await graphContext.ExecuteScalar(cmd);
        addResult.IsOk().Should().BeTrue();

        map.Nodes.Count.Should().Be(2);
        map.Edges.Count.Should().Be(1);
        store.Count.Should().Be(0);
        trace.Count.Should().Be(3);

        var traces = trace.GetTraces();
        traces.Count.Should().Be(3);
        CheckCurrentNode(traces[0], ChangeTrxType.NodeAdd, "node1", "");
        CheckCurrentNode(traces[1], ChangeTrxType.NodeAdd, "node2", "");
        CheckCurrentEdge(traces[2], ChangeTrxType.EdgeAdd, "node1", "node2", "");
    }

    [Fact]
    public async Task AddEdgeFullLifecycle()
    {
        var map = new GraphMap();
        var store = new InMemoryFileStore(NullLogger<InMemoryFileStore>.Instance);
        var trace = new InMemoryChangeTrace();
        var graphContext = new GraphContext(map, store, trace, NullScopeContext.Instance);

        var cmd = """
            add node key=node1;
            add node key=node2;
            add unique edge fromKey=node1, toKey=node2;
            """;

        var addResult = await graphContext.ExecuteScalar(cmd);
        addResult.IsOk().Should().BeTrue();

        map.Nodes.Count.Should().Be(2);
        map.Edges.Count.Should().Be(1);
        store.Count.Should().Be(0);
        trace.Count.Should().Be(3);

        var traces = trace.GetTraces();
        traces.Count.Should().Be(3);
        CheckCurrentNode(traces[0], ChangeTrxType.NodeAdd, "node1", "");
        CheckCurrentNode(traces[1], ChangeTrxType.NodeAdd, "node2", "");
        CheckCurrentEdge(traces[2], ChangeTrxType.EdgeAdd, "node1", "node2", "");

        var updateResult = await graphContext.ExecuteScalar("update [fromKey=node1, toKey=node2] set t1=v1;");
        updateResult.IsOk().Should().BeTrue();
        updateResult.Return().Items.Length.Should().Be(1);

        traces = trace.GetTraces();
        traces.Count.Should().Be(4);
        CheckCurrentNode(traces[0], ChangeTrxType.NodeAdd, "node1", "");
        CheckCurrentNode(traces[1], ChangeTrxType.NodeAdd, "node2", "");
        CheckCurrentEdge(traces[2], ChangeTrxType.EdgeAdd, "node1", "node2", "");
        CheckCurrentAndUpdateEdge(traces[3], ChangeTrxType.EdgeChange, "node1", "node2", "", "t1=v1");

        var deleteResult = await graphContext.ExecuteScalar("delete [fromKey=node1, toKey=node2];");
        deleteResult.IsOk().Should().BeTrue();
        deleteResult.Return().Items.Length.Should().Be(1);

        traces = trace.GetTraces();
        traces.Count.Should().Be(5);
        CheckCurrentNode(traces[0], ChangeTrxType.NodeAdd, "node1", "");
        CheckCurrentNode(traces[1], ChangeTrxType.NodeAdd, "node2", "");
        CheckCurrentEdge(traces[2], ChangeTrxType.EdgeAdd, "node1", "node2", "");
        CheckCurrentAndUpdateEdge(traces[3], ChangeTrxType.EdgeChange, "node1", "node2", "", "t1=v1");

        traces[4].ToObject<ChangeTrx>().NotNull().Action(x =>
        {
            x.TrxType.Should().Be(ChangeTrxType.EdgeDelete);
            x.CurrentNodeValue.Should().BeNull();
            x.UpdateNodeValue.Should().BeNull();
            x.CurrentEdgeValue.NotNull().Action(y =>
            {
                y.FromKey.Should().Be("node1");
                y.ToKey.Should().Be("node2");
                y.Tags.ToTagsString().Should().Be("t1=v1");
            });
            x.UpdateEdgeValue.Should().BeNull();
        });
    }

    private static void CheckCurrentNode(string data, ChangeTrxType trxType, string nodeKey, string tags)
    {
        ChangeTrx trx = data.ToObject<ChangeTrx>().NotNull();
        trx.TrxType.Should().Be(trxType);
        trx.CurrentNodeValue.NotNull().Action(y =>
        {
            y.Key.Should().Be(nodeKey);
            y.Tags.ToTagsString().Should().Be(tags);
        });
        trx.UpdateNodeValue.Should().BeNull();
        trx.CurrentEdgeValue.Should().BeNull();
        trx.UpdateEdgeValue.Should().BeNull();
    }

    private static void CheckCurrentAndUpdateNode(string data, ChangeTrxType trxType, string nodeKey, string tags, string updatedTags)
    {
        ChangeTrx trx = data.ToObject<ChangeTrx>().NotNull();
        trx.TrxType.Should().Be(trxType);
        trx.CurrentNodeValue.NotNull().Action(y =>
        {
            y.Key.Should().Be(nodeKey);
            y.Tags.ToTagsString().Should().Be(tags);
        });
        trx.UpdateNodeValue.NotNull().Action(y =>
        {
            y.Key.Should().Be(nodeKey);
            y.Tags.ToTagsString().Should().Be(updatedTags);
        });
        trx.CurrentEdgeValue.Should().BeNull();
        trx.UpdateEdgeValue.Should().BeNull();
    }

    private static void CheckCurrentEdge(string data, ChangeTrxType trxType, string fromNode, string toNode, string tags)
    {
        ChangeTrx trx = data.ToObject<ChangeTrx>().NotNull();
        trx.TrxType.Should().Be(trxType);
        trx.CurrentNodeValue.Should().BeNull();
        trx.UpdateNodeValue.Should().BeNull();
        trx.CurrentEdgeValue.NotNull().Action(y =>
        {
            y.FromKey.Should().Be(fromNode);
            y.ToKey.Should().Be(toNode);
            y.Tags.ToTagsString().Should().Be(tags);
        });
        trx.UpdateEdgeValue.Should().BeNull();
    }

    private static void CheckCurrentAndUpdateEdge(string data, ChangeTrxType trxType, string fromNode, string toNode, string tags, string updatedTags)
    {
        ChangeTrx trx = data.ToObject<ChangeTrx>().NotNull();
        trx.TrxType.Should().Be(trxType);
        trx.CurrentNodeValue.Should().BeNull();
        trx.UpdateNodeValue.Should().BeNull();
        trx.CurrentEdgeValue.NotNull().Action(y =>
        {
            y.FromKey.Should().Be(fromNode);
            y.ToKey.Should().Be(toNode);
            y.Tags.ToTagsString().Should().Be(tags);
        });
        trx.UpdateEdgeValue.NotNull().Action(y =>
        {
            y.FromKey.Should().Be(fromNode);
            y.ToKey.Should().Be(toNode);
            y.Tags.ToTagsString().Should().Be(updatedTags);
        });
    }
}
