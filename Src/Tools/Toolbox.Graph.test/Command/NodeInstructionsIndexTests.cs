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
    public async Task SetNodeAsSchema()
    {
        var copyMap = _map.Clone();
        var testClient = GraphTestStartup.CreateGraphTestHost(copyMap);
        var newMapOption = await testClient.ExecuteBatch("set node key=provider:provider1/provider1-key set uniqueIndex;", NullScopeContext.Default);
        newMapOption.IsOk().Should().BeTrue(newMapOption.ToString());

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
        var cmd = "set node key=user:username1@company.com set loginProvider=userEmail:username1@domain1.com, email=userEmail:username1@domain1.com index loginProvider, email ;";
        var copyMap = _map.Clone();
        var testClient = GraphTestStartup.CreateGraphTestHost(copyMap);
        var newMapOption = await testClient.ExecuteBatch(cmd, NullScopeContext.Default);
        newMapOption.IsOk().Should().BeTrue(newMapOption.ToString());

        QueryBatchResult commandResults = newMapOption.Return();
        var compareMap = GraphCommandTools.CompareMap(_map, copyMap);

        compareMap.Count.Should().Be(1);
        compareMap[0].Cast<GraphNode>().Action(x =>
        {
            x.Key.Should().Be("user:username1@company.com");
            x.Tags.ToTagsString().Should().Be("email=userEmail:username1@domain1.com,loginProvider=userEmail:username1@domain1.com");
            x.Indexes.Should().Contain("loginProvider");
            x.Indexes.Should().Contain("email");
        });

        commandResults.Items.Count.Should().Be(1);
    }
}
