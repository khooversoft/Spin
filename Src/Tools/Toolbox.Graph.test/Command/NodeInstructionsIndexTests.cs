using Toolbox.Extensions;
using Toolbox.Tools.Should;
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
        await using GraphHostService testClient = await GraphTestStartup.CreateGraphService(_map.Clone());

        var newMapOption = await testClient.ExecuteBatch("set node key=provider:provider1/provider1-key set uniqueIndex;", NullScopeContext.Default);
        newMapOption.IsOk().Should().BeTrue();

        testClient.Map.Nodes.LookupTag("uniqueIndex").Action(x =>
        {
            x.Count.Should().Be(1);
            Enumerable.SequenceEqual(x, ["provider:provider1/provider1-key"]);
        });

        testClient.Map.Meter.Node.GetCount().Should().Be(8);
        testClient.Map.Meter.Node.GetAdded().Should().Be(8);
        testClient.Map.Meter.Node.GetUpdated().Should().Be(0);
        testClient.Map.Meter.Node.GetIndexHit().Should().Be(1);
        testClient.Map.Meter.Node.GetIndexMissed().Should().Be(1);

        QueryBatchResult commandResults = newMapOption.Return();
        var compareMap = GraphCommandTools.CompareMap(_map, testClient.Map);

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
        await using GraphHostService testClient = await GraphTestStartup.CreateGraphService(_map.Clone());

        var cmd = "set node key=user:username1@company.com set loginProvider=userEmail:username1@domain1.com, email=userEmail:username1@domain1.com index loginProvider ;";
        var newMapOption = await testClient.ExecuteBatch(cmd, NullScopeContext.Default);
        newMapOption.IsOk().Should().BeTrue();

        var uniqueIndex = new UniqueIndex("loginProvider", "userEmail", "userEmail:username1@domain1.com");
        testClient.Map.Nodes.LookupByNodeKey("user:username1@company.com").Action(x =>
        {
            x.Count.Should().Be(1);
            Enumerable.SequenceEqual(x, [uniqueIndex]);
        });

        testClient.Map.Nodes.LookupTag("email").Action(x =>
        {
            x.Count.Should().Be(1);
            Enumerable.SequenceEqual(x, ["user:username1@company.com"]);
        });

        testClient.Map.Nodes.LookupIndex("loginProvider", "userEmail:username1@domain1.com").Action(x =>
        {
            x.IsOk().Should().BeTrue();
            x.Return().NodeKey.Should().Be("user:username1@company.com");
        });

        testClient.Map.Meter.Node.GetCount().Should().Be(8);
        testClient.Map.Meter.Node.GetAdded().Should().Be(8);
        testClient.Map.Meter.Node.GetUpdated().Should().Be(0);
        testClient.Map.Meter.Node.GetIndexHit().Should().Be(3);
        testClient.Map.Meter.Node.GetIndexMissed().Should().Be(1);

        QueryBatchResult commandResults = newMapOption.Return();
        var compareMap = GraphCommandTools.CompareMap(_map, testClient.Map);

        compareMap.Count.Should().Be(1);
        compareMap[0].Cast<GraphNode>().Action(x =>
        {
            x.Key.Should().Be("user:username1@company.com");
            x.Tags.ToTagsString().Should().Be("email=userEmail:username1@domain1.com,loginProvider=userEmail:username1@domain1.com");
            x.Indexes.Any(x => x == "loginProvider").Should().BeTrue();
        });

        commandResults.Items.Count.Should().Be(1);
    }

    [Fact]
    public async Task SetNodeWithTwoIndex()
    {
        await using GraphHostService testClient = await GraphTestStartup.CreateGraphService(_map.Clone());

        var cmd = "set node key=user:username1@company.com set loginProvider=provider:provider1/provider1-key, email=userEmail:username1@domain1.com index loginProvider, email ;";
        var newMapOption = await testClient.ExecuteBatch(cmd, NullScopeContext.Default);
        newMapOption.IsOk().Should().BeTrue();

        UniqueIndex[] indexes = [
            new UniqueIndex("email", "userEmail:username1@domain1.com", "user:username1@company.com"),
            new UniqueIndex("loginProvider", "provider:provider1/provider1-key", "user:username1@company.com"),
            ];

        testClient.Map.Nodes.LookupByNodeKey("user:username1@company.com").Action(x =>
        {
            x.Count.Should().Be(indexes.Length);
            var source = x.OrderBy(x => x.PrimaryKey).ToArray();
            var target = indexes.OrderBy(x => x.PrimaryKey).ToArray();

            source.SequenceEqual(target).Should().BeTrue();
        });

        testClient.Map.Nodes.LookupTag("email").Action(x =>
        {
            x.Count.Should().Be(1);
            x.SequenceEqual(["user:username1@company.com"]).Should().BeTrue();
        });

        testClient.Map.Nodes.LookupIndex("loginProvider", "provider:provider1/provider1-key").Action(x =>
        {
            x.IsOk().Should().BeTrue();
            x.Return().NodeKey.Should().Be("user:username1@company.com");
        });

        testClient.Map.Meter.Node.GetCount().Should().Be(8);
        testClient.Map.Meter.Node.GetAdded().Should().Be(8);
        testClient.Map.Meter.Node.GetUpdated().Should().Be(0);
        testClient.Map.Meter.Node.GetIndexHit().Should().Be(3);
        testClient.Map.Meter.Node.GetIndexMissed().Should().Be(1);

        QueryBatchResult commandResults = newMapOption.Return();
        var compareMap = GraphCommandTools.CompareMap(_map, testClient.Map);

        compareMap.Count.Should().Be(1);
        compareMap[0].Cast<GraphNode>().Action(x =>
        {
            x.Key.Should().Be("user:username1@company.com");
            x.Tags.ToTagsString().Should().Be("email=userEmail:username1@domain1.com,loginProvider=provider:provider1/provider1-key");
            x.Indexes.Any(x => x == "loginProvider").Should().BeTrue();
            x.Indexes.Any(x => x == "email").Should().BeTrue();
        });

        commandResults.Items.Count.Should().Be(1);
    }
}
