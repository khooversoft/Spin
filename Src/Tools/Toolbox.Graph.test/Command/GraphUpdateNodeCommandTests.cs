using FluentAssertions;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph.test.Command;

public class GraphUpdateNodeCommandTests
{
    private static readonly GraphMap _map = new GraphMap()
    {
        new GraphNode("node1", tags: "name=marko,age=29"),
        new GraphNode("node2", tags: "name=vadas,age=35"),
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
    public async Task SingleUpdateForNode()
    {
        var workMap = _map.Clone();
        var testClient = GraphTestStartup.CreateGraphTestHost(workMap);
        var newMapOption = await testClient.ExecuteBatch("update (key=node3) set t1;", NullScopeContext.Instance);
        newMapOption.IsOk().Should().BeTrue();

        GraphQueryResults commandResults = newMapOption.Return();
        var compareMap = GraphCommandTools.CompareMap(_map, workMap);

        compareMap.Count.Should().Be(1);
        var index = compareMap.ToCursor();

        index.NextValue().Return().Cast<GraphNode>().Action(x =>
        {
            x.Key.Should().Be("node3");
            x.Tags.ToTagsString().Should().Be("lang=java,name=lop,t1");
        });

        commandResults.Items.Length.Should().Be(1);
        var resultIndex = commandResults.Items.ToCursor();

        resultIndex.NextValue().Return().Action(x =>
        {
            x.CommandType.Should().Be(CommandType.UpdateNode);
            x.Status.IsOk().Should().BeTrue();
            x.Items.NotNull().Length.Should().Be(1);

            var resultIndex = x.Items.NotNull().ToCursor();
            resultIndex.NextValue().Return().Cast<GraphNode>().Action(x =>
            {
                x.Key.Should().Be("node3");
                x.Tags.ToTagsString().Should().Be("lang=java,name=lop");
            });
        });
    }

    [Fact]
    public async Task SingleUpdateForNodeWithData()
    {
        var workMap = _map.Clone();
        var testClient = GraphTestStartup.CreateGraphTestHost(workMap);
        var newMapOption = await testClient.ExecuteBatch("update (key=node3) set contract { 'aGVsbG8=' };", NullScopeContext.Instance);
        newMapOption.IsOk().Should().BeTrue();

        GraphQueryResults commandResults = newMapOption.Return();
        var compareMap = GraphCommandTools.CompareMap(_map, workMap);

        compareMap.Count.Should().Be(1);
        var index = compareMap.ToCursor();

        index.NextValue().Return().Cast<GraphNode>().Action(x =>
        {
            x.Key.Should().Be("node3");
            x.Tags.ToTagsString().Should().Be("lang=java,name=lop");
        });

        commandResults.Items.Length.Should().Be(1);

        GraphQueryResult search = (await testClient.Execute("select (key=node3);", NullScopeContext.Instance)).ThrowOnError().Return();
        search.Status.IsOk().Should().BeTrue();
        search.Items.Length.Should().Be(1);
        search.Items[0].Cast<GraphNode>().Action(x =>
        {
            x.Key.Should().Be("node3");
            x.Tags.Count.Should().Be(2);
            x.DataMap.Count.Should().Be(1);
            x.DataMap.Values.First().Action(y =>
            {
                y.Schema.Should().Be("json");
                y.TypeName.Should().Be("default");
                y.FileId.Should().Be("nodes/node3/node3___contract.json");
            });
        });
    }

    [Fact]
    public async Task SingleRemoveTagForNode()
    {
        var workMap = _map.Clone();
        var testClient = GraphTestStartup.CreateGraphTestHost(workMap);
        var newMapOption = await testClient.ExecuteBatch("update (key=node3) set -name;", NullScopeContext.Instance);
        newMapOption.IsOk().Should().BeTrue();

        GraphQueryResults commandResults = newMapOption.Return();
        var compareMap = GraphCommandTools.CompareMap(_map, workMap);

        compareMap.Count.Should().Be(1);
        var index = compareMap.ToCursor();

        index.NextValue().Return().Cast<GraphNode>().Action(x =>
        {
            x.Key.Should().Be("node3");
            x.Tags.ToTagsString().Should().Be("lang=java");
        });

        commandResults.Items.Length.Should().Be(1);
        var resultIndex = commandResults.Items.ToCursor();

        resultIndex.NextValue().Return().Action(x =>
        {
            x.CommandType.Should().Be(CommandType.UpdateNode);
            x.Status.IsOk().Should().BeTrue();
            x.Items.NotNull().Length.Should().Be(1);

            var resultIndex = x.Items.NotNull().ToCursor();
            resultIndex.NextValue().Return().Cast<GraphNode>().Action(x =>
            {
                x.Key.Should().Be("node3");
                x.Tags.ToTagsString().Should().Be("lang=java,name=lop");
            });
        });
    }
}
