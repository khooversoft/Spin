using Toolbox.Extensions;
using Toolbox.Tools.Should;
using Toolbox.Types;

namespace Toolbox.Graph.test.Command;

public class AddEdgeCommandTests
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

    [Theory]
    [InlineData("add edge fromKey=node4, toKey=node5;")]
    public async Task Failures(string query)
    {
        await using GraphHostService testClient = await GraphTestStartup.CreateGraphService(_map.Clone());

        var newMapOption = await testClient.ExecuteBatch(query, NullScopeContext.Default);
        newMapOption.IsError().Should().BeTrue();

        testClient.Map.Nodes.Count.Should().Be(7);
        testClient.Map.Edges.Count.Should().Be(5);
    }

    [Fact]
    public async Task SingleAddForEdge()
    {
        await using GraphHostService testClient = await GraphTestStartup.CreateGraphService(_map.Clone());

        var newMapOption = await testClient.ExecuteBatch("add edge from=node7, to=node1, type=newEdgeType set newTags;", NullScopeContext.Default);
        newMapOption.IsOk().Should().BeTrue();

        QueryBatchResult commandResults = newMapOption.Return();
        var compareMap = GraphCommandTools.CompareMap(_map, testClient.Map);

        compareMap.Count.Should().Be(1);
        compareMap[0].Cast<GraphEdge>().Action(x =>
        {
            x.FromKey.Should().Be("node7");
            x.ToKey.Should().Be("node1");
            x.EdgeType.Should().Be("newEdgeType");
            x.Tags.ToTagsString().Should().Be("newTags");
        });

        commandResults.Items.Count.Should().Be(1);
        var resultIndex = commandResults.Items.ToCursor();

        resultIndex.NextValue().Return().Action(x =>
        {
            x.Option.IsOk().Should().BeTrue();
            x.Nodes.Count.Should().Be(0);
            x.Edges.Count.Should().Be(0);
            x.DataLinks.Count.Should().Be(0);
        });
    }

    [Fact]
    public async Task SingleAddForEdgeTagsCommand()
    {
        await using GraphHostService testClient = await GraphTestStartup.CreateGraphService(_map.Clone());

        var newMapOption = await testClient.ExecuteBatch("add edge from=node7, to=node1, type=newEdgeType set -newTags;", NullScopeContext.Default);
        newMapOption.IsOk().Should().BeTrue();

        QueryBatchResult commandResults = newMapOption.Return();
        var compareMap = GraphCommandTools.CompareMap(_map, testClient.Map);

        compareMap.Count.Should().Be(1);
        compareMap[0].Cast<GraphEdge>().Action(x =>
        {
            x.FromKey.Should().Be("node7");
            x.ToKey.Should().Be("node1");
            x.EdgeType.Should().Be("newEdgeType");
            x.Tags.Count.Should().Be(0);
        });

        commandResults.Items.Count.Should().Be(1);
    }

    [Fact]
    public async Task SingleUniqueAddForEdge()
    {
        await using GraphHostService testClient = await GraphTestStartup.CreateGraphService(_map.Clone());

        var newMapOption = await testClient.ExecuteBatch("add edge from=node7, to=node1, type=newEdgeType set newTags;", NullScopeContext.Default);
        newMapOption.IsOk().Should().BeTrue();

        QueryBatchResult commandResults = newMapOption.Return();
        var compareMap = GraphCommandTools.CompareMap(_map, testClient.Map);

        compareMap.Count.Should().Be(1);
        compareMap[0].Cast<GraphEdge>().Action(x =>
        {
            x.FromKey.Should().Be("node7");
            x.ToKey.Should().Be("node1");
            x.EdgeType.Should().Be("newEdgeType");
            x.Tags.ToTagsString().Should().Be("newTags");
        });

        commandResults.Items.Count.Should().Be(1);
    }
}
