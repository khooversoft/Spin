using FluentAssertions;
using Toolbox.Extensions;
using Toolbox.Types;

namespace Toolbox.Graph.test.Command;

public class GraphAddEdgeCommandTests
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
    public async Task SingleAddForEdge()
    {
        var copyMap = _map.Clone();
        var testClient = GraphTestStartup.CreateGraphTestHost(copyMap);
        var newMapOption = await testClient.ExecuteBatch("add edge fromKey=node7, toKey=node1, edgeType=newEdgeType, newTags;", NullScopeContext.Instance);
        newMapOption.IsOk().Should().BeTrue(newMapOption.ToString());

        QueryBatchResult commandResults = newMapOption.Return();
        var compareMap = GraphCommandTools.CompareMap(_map, copyMap);

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
            x.Edges.Count.Should().Be(1);
            x.Data.Count.Should().Be(0);
        });
    }

    [Fact]
    public async Task SingleAddForEdgeTagsCommand()
    {
        var copyMap = _map.Clone();
        var testClient = GraphTestStartup.CreateGraphTestHost(copyMap);
        var newMapOption = await testClient.ExecuteBatch("add edge fromKey=node7, toKey=node1, edgeType=newEdgeType, -newTags;", NullScopeContext.Instance);
        newMapOption.IsOk().Should().BeTrue();

        QueryBatchResult commandResults = newMapOption.Return();
        var compareMap = GraphCommandTools.CompareMap(_map, copyMap);

        compareMap.Count.Should().Be(1);
        compareMap[0].Cast<GraphEdge>().Action(x =>
        {
            x.FromKey.Should().Be("node7");
            x.ToKey.Should().Be("node1");
            x.EdgeType.Should().Be("newEdgeType");
            x.Tags.Count.Should().Be(0);
        });

        commandResults.Items.Count.Should().Be(1);
        var resultIndex = commandResults.Items.ToCursor();

        resultIndex.NextValue().Return().Action(x =>
        {
            x.Option.IsOk().Should().BeTrue();
            x.Nodes.Count.Should().Be(0);
            x.Edges.Count.Should().Be(1);
            x.Data.Count.Should().Be(0);
        });
    }

    [Fact]
    public async Task SingleUniqueAddForEdge()
    {
        var copyMap = _map.Clone();
        var testClient = GraphTestStartup.CreateGraphTestHost(copyMap);
        var newMapOption = await testClient.ExecuteBatch("add unique edge fromKey=node7, toKey=node1, edgeType=newEdgeType, newTags;", NullScopeContext.Instance);
        newMapOption.IsOk().Should().BeTrue();

        QueryBatchResult commandResults = newMapOption.Return();
        var compareMap = GraphCommandTools.CompareMap(_map, copyMap);

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
            x.Edges.Count.Should().Be(1);
            x.Data.Count.Should().Be(0);
        });
    }

    [Fact]
    public async Task UniqueAddForEdgeWithExistingEdge()
    {
        var copyMap = _map.Clone();
        var testClient = GraphTestStartup.CreateGraphTestHost(copyMap);
        var newMapOption = await testClient.ExecuteBatch("add unique edge fromKey=node4, toKey=node5;", NullScopeContext.Instance);
        newMapOption.IsError().Should().BeTrue();

        QueryBatchResult commandResults = newMapOption.Return();
        var compareMap = GraphCommandTools.CompareMap(copyMap, _map);

        compareMap.Count.Should().Be(0);

        commandResults.Items.Count.Should().Be(1);
        var resultIndex = commandResults.Items.ToCursor();

        resultIndex.NextValue().Return().Action(x =>
        {
            x.Option.IsOk().Should().BeTrue();
            x.Nodes.Count.Should().Be(0);
            x.Edges.Count.Should().Be(1);
            x.Data.Count.Should().Be(0);
        });
    }
}
