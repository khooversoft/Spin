using FluentAssertions;
using Toolbox.Data;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Test.Data.Graph.Command;

public class GraphSelectCommandTests
{
    private readonly GraphMap _map = new GraphMap()
    {
        new GraphNode("node1", tags: "name=marko;age=29"),
        new GraphNode("node2", tags: "name=vadas;age=27"),
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
    public void SingleSelectForNode()
    {
        var newMapOption = _map.Command().Execute("select (tags='lang=java');");
        newMapOption.IsOk().Should().BeTrue();

        GraphCommandExceuteResults commandResults = newMapOption.Return();

        commandResults.GraphMap.Should().NotBeNull();
        GraphCommandTools.CompareMap(_map, commandResults.GraphMap).Count.Should().Be(0);

        commandResults.Items.Count.Should().Be(1);
        var resultIndex = commandResults.Items.ToCursor();

        resultIndex.NextValue().Return().Action(x =>
        {
            x.CommandType.Should().Be(CommandType.Select);
            x.StatusCode.IsOk().Should().BeTrue();
            x.Error.Should().BeNull();

            x.SearchResult.NotNull().Items.Count.Should().Be(3);
            var index = x.SearchResult.NotNull().Items.ToCursor();

            index.NextValue().Return().Cast<GraphNode>().Action(x =>
            {
                x.Key.Should().Be("node3");
                x.Tags.ToString().Should().Be("lang=java;name=lop");
            });

            index.NextValue().Return().Cast<GraphNode>().Action(x =>
            {
                x.Key.Should().Be("node5");
                x.Tags.ToString().Should().Be("lang=java;name=ripple");
            });

            index.NextValue().Return().Cast<GraphNode>().Action(x =>
            {
                x.Key.Should().Be("node7");
                x.Tags.ToString().Should().Be("lang=java");
            });
        });
    }

    [Fact]
    public void SingleSelectForEdge()
    {
        var newMapOption = _map.Command().Execute("select [knows];");
        newMapOption.IsOk().Should().BeTrue();

        GraphCommandExceuteResults commandResults = newMapOption.Return();

        commandResults.GraphMap.Should().NotBeNull();
        GraphCommandTools.CompareMap(_map, commandResults.GraphMap).Count.Should().Be(0);

        commandResults.Items.Count.Should().Be(1);
        var resultIndex = commandResults.Items.ToCursor();

        resultIndex.NextValue().Return().Action(x =>
        {
            x.CommandType.Should().Be(CommandType.Select);
            x.StatusCode.IsOk().Should().BeTrue();
            x.Error.Should().BeNull();

            x.SearchResult.NotNull().Items.Count.Should().Be(2);

            var index = x.SearchResult.NotNull().Items.ToCursor();
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
        });
    }


    [Fact]
    public void TwoSelectCommandQuery()
    {
        var newMapOption = _map.Command().Execute("select [knows];select [created];");
        newMapOption.IsOk().Should().BeTrue();

        GraphCommandExceuteResults commandResults = newMapOption.Return();

        commandResults.GraphMap.Should().NotBeNull();
        GraphCommandTools.CompareMap(_map, commandResults.GraphMap).Count.Should().Be(0);
        commandResults.Items.Count.Should().Be(2);

        var resultIndex = commandResults.Items.ToCursor();

        resultIndex.NextValue().Return().Action(x =>
        {
            x.CommandType.Should().Be(CommandType.Select);
            x.StatusCode.IsOk().Should().BeTrue();
            x.Error.Should().BeNull();

            x.SearchResult.NotNull().Items.Count.Should().Be(2);
            var index = x.SearchResult.NotNull().Items.ToCursor();

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
        });

        resultIndex.NextValue().Return().Action(x =>
        {
            x.CommandType.Should().Be(CommandType.Select);
            x.StatusCode.IsOk().Should().BeTrue();
            x.Error.Should().BeNull();

            x.SearchResult.NotNull().Items.Count.Should().Be(3);
            var index = x.SearchResult.NotNull().Items.ToCursor();

            index.NextValue().Return().Cast<GraphEdge>().Action(x =>
            {
                x.FromKey.Should().Be("node6");
                x.ToKey.Should().Be("node3");
                x.Tags.ToString().Should().Be("created");
            });

            index.NextValue().Return().Cast<GraphEdge>().Action(x =>
            {
                x.FromKey.Should().Be("node4");
                x.ToKey.Should().Be("node5");
                x.Tags.ToString().Should().Be("created");
            });

            index.NextValue().Return().Cast<GraphEdge>().Action(x =>
            {
                x.FromKey.Should().Be("node4");
                x.ToKey.Should().Be("node3");
                x.Tags.ToString().Should().Be("created");
            });
        });
    }
}
