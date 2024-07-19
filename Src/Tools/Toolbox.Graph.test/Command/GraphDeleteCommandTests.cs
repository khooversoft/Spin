using FluentAssertions;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph.test.Command;

public class GraphDeleteCommandTests
{
    private readonly GraphMap _map = new GraphMap()
    {
        new GraphNode("node1", tags: "name=marko,age=29"),
        new GraphNode("node2", tags: "name=vadas,age=35"),
        new GraphNode("node3", tags: "name=lop,lang=java"),
        new GraphNode("node4", tags: "name=josh,age=32"),
        new GraphNode("node5", tags: "name=ripple,lang=java,marked=true"),
        new GraphNode("node6", tags: "name=peter,age=35"),
        new GraphNode("node7", tags: "lang=java"),

        new GraphEdge("node1", "node2", tags: "knows,level=1"),
        new GraphEdge("node1", "node3", tags: "knows,level=1"),
        new GraphEdge("node6", "node3", tags: "created"),
        new GraphEdge("node4", "node5", tags: "created"),
        new GraphEdge("node4", "node3", tags: "created"),
    };

    [Fact]
    public async Task SingleDeleteForNode()
    {
        var copyMap = _map.Clone();
        var testClient = GraphTestStartup.CreateGraphTestHost(copyMap);
        var newMapOption = await testClient.ExecuteBatch("delete (key=node1);", NullScopeContext.Instance);
        newMapOption.IsOk().Should().BeTrue(newMapOption.ToString());

        GraphQueryResults commandResults = newMapOption.Return();
        var compareMap = GraphCommandTools.CompareMap(_map, copyMap);

        compareMap.Count.Should().Be(3);
        var index = compareMap.ToCursor();

        index.NextValue().Return().Cast<GraphNode>().Action(x =>
        {
            x.Key.Should().Be("node1");
            x.Tags.ToTagsString().Should().Be("age=29,name=marko");
        });

        index.NextValue().Return().Cast<GraphEdge>().Action(x =>
        {
            x.FromKey.Should().Be("node1");
            x.ToKey.Should().Be("node2");
            x.Tags.ToTagsString().Should().Be("knows,level=1");
        });

        index.NextValue().Return().Cast<GraphEdge>().Action(x =>
        {
            x.FromKey.Should().Be("node1");
            x.ToKey.Should().Be("node3");
            x.Tags.ToTagsString().Should().Be("knows,level=1");
        });

        commandResults.Items.Length.Should().Be(1);
        var resultIndex = commandResults.Items.ToCursor();

        resultIndex.NextValue().Return().Action(x =>
        {
            x.CommandType.Should().Be(CommandType.DeleteNode);
            x.Status.IsOk().Should().BeTrue();
            x.Items.NotNull().Count.Should().Be(1);

            var resultIndex = x.Items.NotNull().ToCursor();
            resultIndex.NextValue().Return().Cast<GraphNode>().Action(x =>
            {
                x.Key.Should().Be("node1");
                x.Tags.ToTagsString().Should().Be("age=29,name=marko");
            });
        });
    }


    [Fact]
    public async Task SingleDeleteForEdgeToNode()
    {
        var copyMap = _map.Clone();
        var testClient = GraphTestStartup.CreateGraphTestHost(copyMap);
        var commandResults = (await testClient.Execute("delete [created] -> (lang=java);", NullScopeContext.Instance)).ThrowOnError().Return();
        commandResults.Status.IsOk().Should().BeTrue(commandResults.ToString());
        commandResults.Items.NotNull().Count.Should().Be(2);
        commandResults.CommandType.Should().Be(CommandType.DeleteNode);

        commandResults.Items.OfType<GraphNode>()
            .Select(x => x.Key)
            .OrderBy(x => x)
            .Zip(["node3", "node5"])
            .All(x => x.First == x.Second)
            .Should().BeTrue();

        var compareMap = GraphCommandTools.CompareMap(_map, copyMap);
        compareMap.Count.Should().Be(6);
        compareMap.OfType<GraphNode>().Count().Should().Be(2);
        compareMap.OfType<GraphEdge>().Count().Should().Be(4);
    }

    [Fact]
    public async Task SingleDeleteForEdgeToNode2()
    {
        var copyMap = _map.Clone();
        var testClient = GraphTestStartup.CreateGraphTestHost(copyMap);
        var commandResults = (await testClient.Execute("delete [created] -> (marked=true);", NullScopeContext.Instance)).ThrowOnError().Return();
        commandResults.Status.IsOk().Should().BeTrue(commandResults.ToString());
        commandResults.Items.NotNull().Count.Should().Be(1);
        commandResults.CommandType.Should().Be(CommandType.DeleteNode);

        commandResults.Items.OfType<GraphNode>().First().Key.Should().Be("node5");

        var compareMap = GraphCommandTools.CompareMap(copyMap, _map);
        compareMap.Count.Should().Be(2);
        compareMap.OfType<GraphNode>().Count().Should().Be(1);
        compareMap.OfType<GraphEdge>().Count().Should().Be(1);
    }

    [Fact]
    public async Task SingleDeleteForEdge()
    {
        var copyMap = _map.Clone();
        var testClient = GraphTestStartup.CreateGraphTestHost(copyMap);
        var newMapOption = await testClient.ExecuteBatch("delete [fromKey=node4, toKey=node5];", NullScopeContext.Instance);
        newMapOption.IsOk().Should().BeTrue(newMapOption.ToString());

        GraphQueryResults commandResults = newMapOption.Return();
        var compareMap = GraphCommandTools.CompareMap(_map, copyMap);

        compareMap.Count.Should().Be(1);
        var index = compareMap.ToCursor();

        index.NextValue().Return().Cast<GraphEdge>().Action(x =>
        {
            x.FromKey.Should().Be("node4");
            x.ToKey.Should().Be("node5");
            x.Tags.ToTagsString().Should().Be("created");
        });

        commandResults.Items.Length.Should().Be(1);
        var resultIndex = commandResults.Items.ToCursor();

        resultIndex.NextValue().Return().Action(x =>
        {
            x.CommandType.Should().Be(CommandType.DeleteEdge);
            x.Status.IsOk().Should().BeTrue();
            x.Items.NotNull().Count.Should().Be(1);

            var resultIndex = x.Items.NotNull().ToCursor();
            resultIndex.NextValue().Return().Cast<GraphEdge>().Action(x =>
            {
                x.FromKey.Should().Be("node4");
                x.ToKey.Should().Be("node5");
                x.Tags.ToTagsString().Should().Be("created");
            });
        });
    }
}
