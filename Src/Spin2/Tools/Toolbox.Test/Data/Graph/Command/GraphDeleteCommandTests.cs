using FluentAssertions;
using Toolbox.Data;
using Toolbox.Extensions;
using Toolbox.Types;

namespace Toolbox.Test.Data.Graph.Command;

public class GraphDeleteCommandTests
{
    private readonly GraphMap _map = new GraphMap()
    {
        new GraphNode("node1", tags: "name=marko;age=29"),
        new GraphNode("node2", tags: "name=vadas;age=35"),
        new GraphNode("node3", tags: "name=lop;lang=java"),
        new GraphNode("node4", tags: "name=josh;age=32"),
        new GraphNode("node5", tags: "name=ripple;lang=java"),
        new GraphNode("node6", tags: "name=peter;age=35"),
        new GraphNode("node7", tags: "lang=java"),

        new GraphEdge("node1", "node2", tags: "knows;level=1"),
        new GraphEdge("node1", "node3", tags: "knows;level=1"),
        new GraphEdge("node6", "node3", tags: "created"),
        new GraphEdge("node4", "node5", tags: "created"),
        new GraphEdge("node4", "node3", tags: "created"),
    };

    [Fact]
    public void SingleDeleteForNode()
    {
        var newMapOption = _map.Command().Execute("delete (key=node1);");
        newMapOption.IsOk().Should().BeTrue();

        CommandResults commandResults = newMapOption.Return();

        commandResults.GraphMap.Should().NotBeNull();
        var compareMap = GraphCommandTools.CompareMap(_map, commandResults.GraphMap);

        compareMap.Count.Should().Be(3);
        var index = compareMap.ToCursor();

        index.NextValue().Return().Cast<GraphNode>().Action(x =>
        {
            x.Key.Should().Be("node1");
            x.Tags.ToString().Should().Be("name=marko;age=29");
        });

        index.NextValue().Return().Cast<GraphEdge>().Action(x =>
        {
            x.FromKey.Should().Be("node1");
            x.ToKey.Should().Be("node2");
            x.Tags.ToString().Should().Be("knows;level=1");
        });

        index.NextValue().Return().Cast<GraphEdge>().Action(x =>
        {
            x.FromKey.Should().Be("node1");
            x.ToKey.Should().Be("node3");
            x.Tags.ToString().Should().Be("knows;level=1");
        });

        commandResults.Results.Count.Should().Be(1);
        var resultIndex = commandResults.Results.ToCursor();

        resultIndex.NextValue().Return().Action(x =>
        {
            x.CommandType.Should().Be(CommandType.DeleteNode);
            x.StatusCode.IsOk().Should().BeTrue();
            x.Error.Should().BeNull();
            x.SearchResult.Count.Should().Be(1);

            var resultIndex = x.SearchResult.ToCursor();
            resultIndex.NextValue().Return().Cast<GraphNode>().Action(x =>
            {
                x.Key.Should().Be("node1");
                x.Tags.ToString().Should().Be("name=marko;age=29");
            });
        });
    }


    [Fact]
    public void SingleDeleteForEdgeToNode()
    {
        var newMapOption = _map.Command().Execute("delete [created] -> (tags='age=35');");
        newMapOption.IsOk().Should().BeTrue();

        CommandResults commandResults = newMapOption.Return();

        commandResults.GraphMap.Should().NotBeNull();
        var compareMap = GraphCommandTools.CompareMap(_map, commandResults.GraphMap);

        compareMap.Count.Should().Be(2);
        var index = compareMap.ToCursor();

        index.NextValue().Return().Cast<GraphNode>().Action(x =>
        {
            x.Key.Should().Be("node6");
            x.Tags.ToString().Should().Be("name=peter;age=35");
        });

        index.NextValue().Return().Cast<GraphEdge>().Action(x =>
        {
            x.FromKey.Should().Be("node6");
            x.ToKey.Should().Be("node3");
            x.Tags.ToString().Should().Be("created");
        });

        commandResults.Results.Count.Should().Be(1);
        var resultIndex = commandResults.Results.ToCursor();

        resultIndex.NextValue().Return().Action(x =>
        {
            x.CommandType.Should().Be(CommandType.DeleteNode);
            x.StatusCode.IsOk().Should().BeTrue();
            x.Error.Should().BeNull();
            x.SearchResult.Count.Should().Be(1);

            var resultIndex = x.SearchResult.ToCursor();
            resultIndex.NextValue().Return().Cast<GraphNode>().Action(x =>
            {
                x.Key.Should().Be("node6");
                x.Tags.ToString().Should().Be("name=peter;age=35");
            });
        });
    }

    [Fact]
    public void SingleDeleteForEdge()
    {
        var newMapOption = _map.Command().Execute("delete [fromKey=node4; toKey=node5];");
        newMapOption.IsOk().Should().BeTrue(newMapOption.ToString());

        CommandResults commandResults = newMapOption.Return();

        commandResults.GraphMap.Should().NotBeNull();
        var compareMap = GraphCommandTools.CompareMap(_map, commandResults.GraphMap);

        compareMap.Count.Should().Be(1);
        var index = compareMap.ToCursor();

        index.NextValue().Return().Cast<GraphEdge>().Action(x =>
        {
            x.FromKey.Should().Be("node4");
            x.ToKey.Should().Be("node5");
            x.Tags.ToString().Should().Be("created");
        });

        commandResults.Results.Count.Should().Be(1);
        var resultIndex = commandResults.Results.ToCursor();

        resultIndex.NextValue().Return().Action(x =>
        {
            x.CommandType.Should().Be(CommandType.DeleteEdge);
            x.StatusCode.IsOk().Should().BeTrue();
            x.Error.Should().BeNull();
            x.SearchResult.Count.Should().Be(1);

            var resultIndex = x.SearchResult.ToCursor();
            resultIndex.NextValue().Return().Cast<GraphEdge>().Action(x =>
            {
                x.FromKey.Should().Be("node4");
                x.ToKey.Should().Be("node5");
                x.Tags.ToString().Should().Be("created");
            });
        });
    }
}
