using Microsoft.Extensions.DependencyInjection;
using Toolbox.Extensions;
using Toolbox.Graph.test.Application;
using Toolbox.Tools.Should;
using Toolbox.Types;
using Xunit.Abstractions;

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

    private readonly ITestOutputHelper _outputHelper;
    public NodeInstructionsIndexTests(ITestOutputHelper outputHelper) => _outputHelper = outputHelper;

    [Fact]
    public async Task TestIndexCounter()
    {
        using GraphHostService testClient = await TestApplication.CreateTestGraphService(_map.Clone(), _outputHelper);
        var collector = testClient.Services.GetRequiredService<GraphMapCounter>();

        collector.Nodes.Count.Value.Should().Be(7);
        collector.Nodes.Added.Value.Should().Be(7);
        collector.Nodes.Deleted.Value.Should().Be(0);
        collector.Nodes.Updated.Value.Should().Be(0);
        collector.Nodes.IndexHit.Value.Should().Be(0);
        collector.Nodes.IndexMissed.Value.Should().Be(0);

        collector.Edges.Count.Value.Should().Be(5);
        collector.Edges.Added.Value.Should().Be(5);
        collector.Edges.Deleted.Value.Should().Be(0);
        collector.Edges.Updated.Value.Should().Be(0);
        collector.Edges.IndexHit.Value.Should().Be(0);
        collector.Edges.IndexMissed.Value.Should().Be(0);
    }

    [Fact]
    public async Task SetNode()
    {
        using GraphHostService testClient = await TestApplication.CreateTestGraphService(_map.Clone(), _outputHelper);
        var collector = testClient.Services.GetRequiredService<GraphMapCounter>();
        var context = testClient.CreateScopeContext<NodeInstructionsIndexTests>();

        var newMapOption = await testClient.ExecuteBatch("set node key=provider:provider1/provider1-key set uniqueIndex;", NullScopeContext.Default);
        newMapOption.IsOk().Should().BeTrue();

        testClient.Map.Nodes.LookupTag("uniqueIndex").Action(x =>
        {
            x.Count.Should().Be(1);
            Enumerable.SequenceEqual(x, ["provider:provider1/provider1-key"]);
        });

        collector.Nodes.Count.Value.Should().Be(8);
        collector.Nodes.Added.Value.Should().Be(8);
        collector.Nodes.Updated.Value.Should().Be(0);
        collector.Nodes.IndexHit.Value.Should().Be(1);
        collector.Nodes.IndexMissed.Value.Should().Be(1);

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
        using GraphHostService testClient = await TestApplication.CreateTestGraphService(_map.Clone(), _outputHelper);
        var collector = testClient.Services.GetRequiredService<GraphMapCounter>();
        var context = testClient.CreateScopeContext<NodeForeignKeyTests>();

        var cmd = "set node key=user:username1@company.com set loginProvider=userEmail:username1@domain1.com, email=userEmail:username1@domain1.com index loginProvider ;";
        var newMapOption = await testClient.ExecuteBatch(cmd, context);
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

        collector.Nodes.Count.Value.Should().Be(8);
        collector.Nodes.Added.Value.Should().Be(8);
        collector.Nodes.Updated.Value.Should().Be(0);
        collector.Nodes.IndexHit.Value.Should().Be(3);
        collector.Nodes.IndexMissed.Value.Should().Be(1);

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
        using GraphHostService testClient = await TestApplication.CreateTestGraphService(_map.Clone(), _outputHelper);
        var collector = testClient.Services.GetRequiredService<GraphMapCounter>();
        var context = testClient.CreateScopeContext<NodeForeignKeyTests>();

        var cmd = "set node key=user:username1@company.com set loginProvider=provider:provider1/provider1-key, email=userEmail:username1@domain1.com index loginProvider, email ;";
        var newMapOption = await testClient.ExecuteBatch(cmd, context);
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

        collector.Nodes.Count.Value.Should().Be(8);
        collector.Nodes.Added.Value.Should().Be(8);
        collector.Nodes.Updated.Value.Should().Be(0);
        collector.Nodes.IndexHit.Value.Should().Be(3);
        collector.Nodes.IndexMissed.Value.Should().Be(1);

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
