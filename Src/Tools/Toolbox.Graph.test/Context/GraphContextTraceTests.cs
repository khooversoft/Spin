using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Store;
using Toolbox.Graph;
using Toolbox.Types;
using FluentAssertions;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace Toolbox.Graph.test.Context;

public class GraphContextTraceTests
{
    [Fact]
    public void ChangeTrxSerialization()
    {
        var graphNode = new GraphNode("node1", "tag1=v1", DateTime.Now, ["link1", "link2"]);
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
        var store = new InMemoryFileStore();
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
    public async Task FullNodeLifeCycle()
    {
        var map = new GraphMap();
        var store = new InMemoryFileStore();
        var trace = new InMemoryChangeTrace();
        var graphContext = new GraphContext(map, store, trace, NullScopeContext.Instance);

        var addResult = await graphContext.ExecuteScalar("add node key=node1;");
        addResult.IsOk().Should().BeTrue();

        map.Nodes.Count.Should().Be(1);
        map.Edges.Count.Should().Be(0);
        store.Count.Should().Be(0);
        trace.Count.Should().Be(1);

        var traces = trace.GetTraces();
        traces[0].ToObject<ChangeTrx>().NotNull().Action(x =>
        {
            x.TrxType.Should().Be(ChangeTrxType.NodeAdd);
            x.CurrentNodeValue.NotNull().Action(y =>
            {
                y.Key.Should().Be("node1");
                y.Tags.ToString().Should().Be(string.Empty);
            });
        });

        var updateResult = await graphContext.ExecuteScalar("update (key=node1) set t1=v1;");
        updateResult.IsOk().Should().BeTrue();

        map.Nodes.Count.Should().Be(1);
        map.Edges.Count.Should().Be(0);
        store.Count.Should().Be(0);
        trace.Count.Should().Be(2);

        traces = trace.GetTraces();
        traces[0].ToObject<ChangeTrx>().NotNull().Action(x =>
        {
            x.TrxType.Should().Be(ChangeTrxType.NodeAdd);
            x.CurrentNodeValue.NotNull().Action(y =>
            {
                y.Key.Should().Be("node1");
                y.Tags.ToString().Should().Be(string.Empty);
            });
        });
        traces[1].ToObject<ChangeTrx>().NotNull().Action(x =>
        {
            x.TrxType.Should().Be(ChangeTrxType.NodeChange);
            x.CurrentNodeValue.NotNull().Action(y =>
            {
                y.Key.Should().Be("node1");
                y.Tags.ToString().Should().Be(string.Empty);
            });
            x.UpdateNodeValue.NotNull().Action(y =>
            {
                y.Key.Should().Be("node1");
                y.Tags.ToString().Should().Be("t1=v1");
            });
        });

        var deleteResult = await graphContext.ExecuteScalar("delete (key=node1);");
        deleteResult.IsOk().Should().BeTrue();

        map.Nodes.Count.Should().Be(0);
        map.Edges.Count.Should().Be(0);
        store.Count.Should().Be(0);
        trace.Count.Should().Be(3);

        traces = trace.GetTraces();
        traces[0].ToObject<ChangeTrx>().NotNull().Action(x =>
        {
            x.TrxType.Should().Be(ChangeTrxType.NodeAdd);
            x.CurrentNodeValue.NotNull().Action(y =>
            {
                y.Key.Should().Be("node1");
                y.Tags.ToString().Should().Be(string.Empty);
            });
            x.UpdateNodeValue.Should().BeNull();
        });
        traces[1].ToObject<ChangeTrx>().NotNull().Action(x =>
        {
            x.TrxType.Should().Be(ChangeTrxType.NodeChange);
            x.CurrentNodeValue.NotNull().Action(y =>
            {
                y.Key.Should().Be("node1");
                y.Tags.ToString().Should().Be(string.Empty);
            });
            x.UpdateNodeValue.NotNull().Action(y =>
            {
                y.Key.Should().Be("node1");
                y.Tags.ToString().Should().Be("t1=v1");
            });
        });

        traces[2].ToObject<ChangeTrx>().NotNull().Action(x =>
        {
            x.TrxType.Should().Be(ChangeTrxType.NodeDelete);
            x.CurrentNodeValue.NotNull().Action(y =>
            {
                y.Key.Should().Be("node1");
                y.Tags.ToString().Should().Be("t1=v1");
            });
            x.UpdateNodeValue.Should().BeNull();
        });

    }

    [Fact]
    public async Task SimpleAddEdge()
    {
        var map = new GraphMap();
        var store = new InMemoryFileStore();
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
        traces[0].ToObject<ChangeTrx>().Action(x =>
        {
            x.TrxType.Should().Be(ChangeTrxType.NodeAdd);
            x.CurrentNodeValue.NotNull().Key.Should().Be("node1");
        });
        traces[1].ToObject<ChangeTrx>().Action(x =>
        {
            x.TrxType.Should().Be(ChangeTrxType.NodeAdd);
            x.CurrentNodeValue.NotNull().Key.Should().Be("node2");
        });
        traces[2].ToObject<ChangeTrx>().Action(x =>
        {
            x.TrxType.Should().Be(ChangeTrxType.EdgeAdd);
            x.CurrentEdgeValue.NotNull().FromKey.Should().Be("node1");
            x.CurrentEdgeValue.NotNull().ToKey.Should().Be("node2");
        });
    }

    [Fact]
    public async Task AddEdgeFullLifecycle()
    {
        var map = new GraphMap();
        var store = new InMemoryFileStore();
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
        traces[0].ToObject<ChangeTrx>().Action(x =>
        {
            x.TrxType.Should().Be(ChangeTrxType.NodeAdd);
            x.CurrentNodeValue.NotNull().Key.Should().Be("node1");
        });
        traces[1].ToObject<ChangeTrx>().Action(x =>
        {
            x.TrxType.Should().Be(ChangeTrxType.NodeAdd);
            x.CurrentNodeValue.NotNull().Key.Should().Be("node2");
        });
        traces[2].ToObject<ChangeTrx>().Action(x =>
        {
            x.TrxType.Should().Be(ChangeTrxType.EdgeAdd);
            x.CurrentEdgeValue.NotNull().FromKey.Should().Be("node1");
            x.CurrentEdgeValue.NotNull().ToKey.Should().Be("node2");
        });
    }
}
