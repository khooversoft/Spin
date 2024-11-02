using FluentAssertions;
using Toolbox.Extensions;
using Toolbox.Types;

namespace Toolbox.Graph.test.Command;

public class NodeInstructionsIndexTests
{
    private readonly GraphMap _map = new GraphMap()
    {
        new GraphNode("node1", tags: "name=marko,age=29"),
        new GraphNode("node2", tags: "name=vadas,age=27"),
        new GraphNode("node3", tags: "name=lop,lang=java"),
        new GraphNode("node4", tags: "name=josh,age=32"),
        new GraphNode("node5", tags: "name=ripple,lang=java"),
        new GraphNode("node6", tags: "name=peter,age=35"),
        new GraphNode("node7", tags: "lang=java"),

        new GraphEdge("node1", "node2", edgeType: "et1", tags: "knows,level=1"),
        new GraphEdge("node1", "node3", edgeType: "et1", tags: "knows,level=1"),
        new GraphEdge("node6", "node3", edgeType: "et1", tags: "created"),
        new GraphEdge("node4", "node5", edgeType: "et1", tags: "created"),
        new GraphEdge("node4", "node3", edgeType : "et1", tags: "created"),
    };

    [Fact]
    public void TestIndexCounter()
    {
        _map.Meter.Node.GetCount().Should().Be(7);
        _map.Meter.Node.GetAdded().Should().Be(7);
        _map.Meter.Node.GetDeleted().Should().Be(0);
        _map.Meter.Node.GetUpdated().Should().Be(0);
        _map.Meter.Node.GetIndexHit().Should().Be(0);
        _map.Meter.Node.GetIndexMissed().Should().Be(0);

        _map.Meter.Edge.GetCount().Should().Be(5);
        _map.Meter.Edge.GetAdded().Should().Be(5);
        _map.Meter.Edge.GetDeleted().Should().Be(0);
        _map.Meter.Edge.GetUpdated().Should().Be(0);
        _map.Meter.Edge.GetIndexHit().Should().Be(0);
        _map.Meter.Edge.GetIndexMissed().Should().Be(0);
    }

    [Fact]
    public async Task SetNode()
    {
        var copyMap = _map.Clone();
        var testClient = GraphTestStartup.CreateGraphTestHost(copyMap);
        var newMapOption = await testClient.ExecuteBatch("set node key=provider:provider1/provider1-key set uniqueIndex;", NullScopeContext.Default);
        newMapOption.IsOk().Should().BeTrue(newMapOption.ToString());

        copyMap.Nodes.LookupTag("uniqueIndex").Action(x =>
        {
            x.Count.Should().Be(1);
            Enumerable.SequenceEqual(x, ["provider:provider1/provider1-key"]);
        });

        copyMap.Meter.Node.GetCount().Should().Be(8);
        copyMap.Meter.Node.GetAdded().Should().Be(8);
        copyMap.Meter.Node.GetUpdated().Should().Be(0);
        copyMap.Meter.Node.GetIndexHit().Should().Be(1);
        copyMap.Meter.Node.GetIndexMissed().Should().Be(1);

        QueryBatchResult commandResults = newMapOption.Return();
        var compareMap = GraphCommandTools.CompareMap(_map, copyMap);

        compareMap.Count.Should().Be(1);
        compareMap[0].Cast<GraphNode>().Action(x =>
        {
            x.Key.Should().Be("provider:provider1/provider1-key");
            x.Tags.ToTagsString().Should().Be("uniqueIndex");
        });

        commandResults.Items.Count.Should().Be(1);
    }

    [Fact]
    public async Task SetNodeWithIndex()
    {
        var cmd = "set node key=user:username1@company.com set loginProvider=userEmail:username1@domain1.com, email=userEmail:username1@domain1.com index loginProvider ;";
        var copyMap = _map.Clone();
        var testClient = GraphTestStartup.CreateGraphTestHost(copyMap);
        var newMapOption = await testClient.ExecuteBatch(cmd, NullScopeContext.Default);
        newMapOption.IsOk().Should().BeTrue(newMapOption.ToString());

        var uniqueIndex = new UniqueIndex("loginProvider", "userEmail", "userEmail:username1@domain1.com");
        copyMap.Nodes.LookupByNodeKey("user:username1@company.com").Action(x =>
        {
            x.Count.Should().Be(1);
            Enumerable.SequenceEqual(x, [uniqueIndex]);
        });

        copyMap.Nodes.LookupTag("email").Action(x =>
        {
            x.Count.Should().Be(1);
            Enumerable.SequenceEqual(x, ["user:username1@company.com"]);
        });

        copyMap.Nodes.LookupIndex("loginProvider", "userEmail:username1@domain1.com").Action(x =>
        {
            x.IsOk().Should().BeTrue();
            x.Return().NodeKey.Should().Be("user:username1@company.com");
        });

        copyMap.Meter.Node.GetCount().Should().Be(8);
        copyMap.Meter.Node.GetAdded().Should().Be(8);
        copyMap.Meter.Node.GetUpdated().Should().Be(0);
        copyMap.Meter.Node.GetIndexHit().Should().Be(3);
        copyMap.Meter.Node.GetIndexMissed().Should().Be(1);

        QueryBatchResult commandResults = newMapOption.Return();
        var compareMap = GraphCommandTools.CompareMap(_map, copyMap);

        compareMap.Count.Should().Be(1);
        compareMap[0].Cast<GraphNode>().Action(x =>
        {
            x.Key.Should().Be("user:username1@company.com");
            x.Tags.ToTagsString().Should().Be("email=userEmail:username1@domain1.com,loginProvider=userEmail:username1@domain1.com");
            x.Indexes.Should().Contain("loginProvider");
        });

        commandResults.Items.Count.Should().Be(1);
    }

    [Fact]
    public async Task SetNodeWithTwoIndex()
    {
        var cmd = "set node key=user:username1@company.com set loginProvider=provider:provider1/provider1-key, email=userEmail:username1@domain1.com index loginProvider, email ;";
        var copyMap = _map.Clone();
        var testClient = GraphTestStartup.CreateGraphTestHost(copyMap);
        var newMapOption = await testClient.ExecuteBatch(cmd, NullScopeContext.Default);
        newMapOption.IsOk().Should().BeTrue(newMapOption.ToString());

        UniqueIndex[] indexes = [
            new UniqueIndex("loginProvider", "provider", "provider:provider1/provider1-key"),
            new UniqueIndex("email", "userEmail", "userEmail:username1@domain1.com"),
            ];

        copyMap.Nodes.LookupByNodeKey("user:username1@company.com").Action(x =>
        {
            x.Count.Should().Be(indexes.Length);
            Enumerable.SequenceEqual(x, indexes);
        });

        copyMap.Nodes.LookupTag("email").Action(x =>
        {
            x.Count.Should().Be(1);
            Enumerable.SequenceEqual(x, ["userEmail:username1@domain1.com"]);
        });

        copyMap.Nodes.LookupIndex("loginProvider", "provider:provider1/provider1-key").Action(x =>
        {
            x.IsOk().Should().BeTrue();
            x.Return().NodeKey.Should().Be("user:username1@company.com");
        });

        copyMap.Meter.Node.GetCount().Should().Be(8);
        copyMap.Meter.Node.GetAdded().Should().Be(8);
        copyMap.Meter.Node.GetUpdated().Should().Be(0);
        copyMap.Meter.Node.GetIndexHit().Should().Be(3);
        copyMap.Meter.Node.GetIndexMissed().Should().Be(1);

        QueryBatchResult commandResults = newMapOption.Return();
        var compareMap = GraphCommandTools.CompareMap(_map, copyMap);

        compareMap.Count.Should().Be(1);
        compareMap[0].Cast<GraphNode>().Action(x =>
        {
            x.Key.Should().Be("user:username1@company.com");
            x.Tags.ToTagsString().Should().Be("email=userEmail:username1@domain1.com,loginProvider=provider:provider1/provider1-key");
            x.Indexes.Should().Contain("loginProvider");
            x.Indexes.Should().Contain("email");
        });

        commandResults.Items.Count.Should().Be(1);
    }
}
