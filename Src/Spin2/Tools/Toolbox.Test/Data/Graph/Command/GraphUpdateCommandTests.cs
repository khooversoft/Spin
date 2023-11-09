using FluentAssertions;
using Toolbox.Data;
using Toolbox.Extensions;
using Toolbox.Types;

namespace Toolbox.Test.Data.Graph.Command;

public class GraphUpdateCommandTests
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
    public void SingleUpdateForNode()
    {
        var newMapOption = _map.Command().Execute("update (key=node3) set tags=t1;");
        newMapOption.IsOk().Should().BeTrue();

        CommandResults commandResults = newMapOption.Return();

        commandResults.GraphMap.Should().NotBeNull();
        var compareMap = GraphCommandTools.CompareMap(_map, commandResults.GraphMap);

        compareMap.Count.Should().Be(1);
        var index = compareMap.ToCursor();

        index.NextValue().Return().Cast<GraphNode>().Action(x =>
        {
            x.Key.Should().Be("node3");
            x.Tags.ToString().Should().Be("name=lop;lang=java;t1");
        });

        commandResults.Results.Count.Should().Be(1);
        var resultIndex = commandResults.Results.ToCursor();

        resultIndex.NextValue().Return().Action(x =>
        {
            x.CommandType.Should().Be(CommandType.UpdateNode);
            x.StatusCode.IsOk().Should().BeTrue();
            x.Error.Should().BeNull();
            x.SearchResult.Count.Should().Be(1);

            var resultIndex = x.SearchResult.ToCursor();
            resultIndex.NextValue().Return().Cast<GraphNode>().Action(x =>
            {
                x.Key.Should().Be("node3");
                x.Tags.ToString().Should().Be("name=lop;lang=java;t1");
            });
        });
    }

    [Fact]
    public void SingleUpdateForEdge()
    {
        var newMapOption = _map.Command().Execute("update [fromKey=node4;toKey=node5] set tags=t1;");
        newMapOption.IsOk().Should().BeTrue();

        CommandResults commandResults = newMapOption.Return();

        commandResults.GraphMap.Should().NotBeNull();
        var compareMap = GraphCommandTools.CompareMap(_map, commandResults.GraphMap);

        compareMap.Count.Should().Be(1);
        var index = compareMap.ToCursor();

        index.NextValue().Return().Cast<GraphEdge>().Action(x =>
        {
            x.FromKey.Should().Be("node4");
            x.ToKey.Should().Be("node5");
            x.EdgeType.Should().Be("default");
            x.Tags.ToString().Should().Be("created;t1");
        });

        commandResults.Results.Count.Should().Be(1);
        var resultIndex = commandResults.Results.ToCursor();

        resultIndex.NextValue().Return().Action(x =>
        {
            x.CommandType.Should().Be(CommandType.UpdateEdge);
            x.StatusCode.IsOk().Should().BeTrue();
            x.Error.Should().BeNull();
            x.SearchResult.Count.Should().Be(1);

            var resultIndex = x.SearchResult.ToCursor();
            resultIndex.NextValue().Return().Cast<GraphEdge>().Action(x =>
            {
                x.FromKey.Should().Be("node4");
                x.ToKey.Should().Be("node5");
                x.EdgeType.Should().Be("default");
                x.Tags.ToString().Should().Be("created;t1");
            });
        });
    }

}
