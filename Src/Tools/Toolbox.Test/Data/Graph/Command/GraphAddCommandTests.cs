using FluentAssertions;
using Toolbox.Data;
using Toolbox.Extensions;
using Toolbox.Types;

namespace Toolbox.Test.Data.Graph.Command;

public class GraphAddCommandTests
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

        new GraphEdge("node1", "node2", tags: "knows,level=1"),
        new GraphEdge("node1", "node3", tags: "knows,level=1"),
        new GraphEdge("node6", "node3", tags: "created"),
        new GraphEdge("node4", "node5", tags: "created"),
        new GraphEdge("node4", "node3", tags: "created"),
    };

    [Fact]
    public void SingleAddForNode()
    {
        var copyMap = _map.Copy();
        var newMapOption = _map.Execute("add node key=node99,tags=newTags;");
        newMapOption.IsOk().Should().BeTrue();

        GraphQueryResults commandResults = newMapOption.Return();
        var compareMap = GraphCommandTools.CompareMap(copyMap, _map);

        compareMap.Count.Should().Be(1);
        compareMap[0].Cast<GraphNode>().Action(x =>
        {
            x.Key.Should().Be("node99");
            x.Tags.ToString().Should().Be("newTags");
        });

        commandResults.Items.Count.Should().Be(1);
        var resultIndex = commandResults.Items.ToCursor();

        resultIndex.NextValue().Return().Action(x =>
        {
            x.CommandType.Should().Be(CommandType.AddNode);
            x.StatusCode.IsOk().Should().BeTrue();
            x.Error.Should().BeNull();
            x.Items.Should().NotBeNull();
        });
    }
     
    [Fact]
    public void SingleAddForNodeWithMultipleTags()
    {
        var copyMap = _map.Copy();
        var newMapOption = _map.Execute("add node key=node99, tags='newTags,label=client';");
        newMapOption.IsOk().Should().BeTrue();

        GraphQueryResults commandResults = newMapOption.Return();
        var compareMap = GraphCommandTools.CompareMap(copyMap, _map);

        compareMap.Count.Should().Be(1);
        compareMap[0].Cast<GraphNode>().Action(x =>
        {
            x.Key.Should().Be("node99");
            x.Tags.ToString().Should().Be("label=client,newTags");
        });

        commandResults.Items.Count.Should().Be(1);
        var resultIndex = commandResults.Items.ToCursor();

        resultIndex.NextValue().Return().Action(x =>
        {
            x.CommandType.Should().Be(CommandType.AddNode);
            x.StatusCode.IsOk().Should().BeTrue();
            x.Error.Should().BeNull();
            x.Items.Should().NotBeNull();
        });
    }

    [Fact]
    public void SingleAddForEdge()
    {
        var copyMap = _map.Copy();
        var newMapOption = _map.Execute("add edge fromKey=node7, toKey=node1, edgeType=newEdgeType, tags=newTags;");
        newMapOption.IsOk().Should().BeTrue();

        GraphQueryResults commandResults = newMapOption.Return();
        var compareMap = GraphCommandTools.CompareMap(copyMap, _map);

        compareMap.Count.Should().Be(1);
        compareMap[0].Cast<GraphEdge>().Action(x =>
        {
            x.FromKey.Should().Be("node7");
            x.ToKey.Should().Be("node1");
            x.EdgeType.Should().Be("newEdgeType");
            x.Tags.ToString().Should().Be("newTags");
        });

        commandResults.Items.Count.Should().Be(1);
        var resultIndex = commandResults.Items.ToCursor();

        resultIndex.NextValue().Return().Action(x =>
        {
            x.CommandType.Should().Be(CommandType.AddEdge);
            x.StatusCode.IsOk().Should().BeTrue();
            x.Error.Should().BeNull();
            x.Items.Should().NotBeNull();
        });
    }
}
