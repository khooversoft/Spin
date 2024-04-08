using FluentAssertions;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph.test.Command;

public class GraphUpdateCommandTests
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
    public void SingleUpdateForNode()
    {
        var workMap = _map.Copy();
        var newMapOption = workMap.Execute("update (key=node3) set t1;", NullScopeContext.Instance);
        newMapOption.IsOk().Should().BeTrue();

        GraphQueryResults commandResults = newMapOption.Return();
        var compareMap = GraphCommandTools.CompareMap(_map, workMap);

        compareMap.Count.Should().Be(1);
        var index = compareMap.ToCursor();

        index.NextValue().Return().Cast<GraphNode>().Action(x =>
        {
            x.Key.Should().Be("node3");
            x.Tags.ToString().Should().Be("lang=java,name=lop,t1");
        });

        commandResults.Items.Count.Should().Be(1);
        var resultIndex = commandResults.Items.ToCursor();

        resultIndex.NextValue().Return().Action(x =>
        {
            x.CommandType.Should().Be(CommandType.UpdateNode);
            x.Status.IsOk().Should().BeTrue();
            x.Items.NotNull().Count.Should().Be(1);

            var resultIndex = x.Items.NotNull().ToCursor();
            resultIndex.NextValue().Return().Cast<GraphNode>().Action(x =>
            {
                x.Key.Should().Be("node3");
                x.Tags.ToString().Should().Be("lang=java,name=lop");
            });
        });
    }


    [Fact]
    public void SingleUpdateForNodeWithLink()
    {
        var workMap = _map.Copy();
        var newMapOption = workMap.Execute("update (key=node3) set link=abc/def;", NullScopeContext.Instance);
        newMapOption.IsOk().Should().BeTrue();

        GraphQueryResults commandResults = newMapOption.Return();
        var compareMap = GraphCommandTools.CompareMap(_map, workMap);

        compareMap.Count.Should().Be(1);
        var index = compareMap.ToCursor();

        index.NextValue().Return().Cast<GraphNode>().Action(x =>
        {
            x.Key.Should().Be("node3");
            x.Tags.ToString().Should().Be("lang=java,name=lop,t1");
            x.Links.Join(',').Should().Be("abc/def");
        });

        commandResults.Items.Count.Should().Be(1);
        var resultIndex = commandResults.Items.ToCursor();

        resultIndex.NextValue().Return().Action(x =>
        {
            x.CommandType.Should().Be(CommandType.UpdateNode);
            x.Status.IsOk().Should().BeTrue();
            x.Items.NotNull().Count.Should().Be(1);

            var resultIndex = x.Items.NotNull().ToCursor();
            resultIndex.NextValue().Return().Cast<GraphNode>().Action(x =>
            {
                x.Key.Should().Be("node3");
                x.Tags.ToString().Should().Be("lang=java,name=lop");
                x.Links.Length.Should().Be(0);
            });
        });
    }

    [Fact]
    public void SingleRemoveTagForNode()
    {
        var workMap = _map.Copy();
        var newMapOption = workMap.Execute("update (key=node3) set -name;", NullScopeContext.Instance);
        newMapOption.IsOk().Should().BeTrue();

        GraphQueryResults commandResults = newMapOption.Return();
        var compareMap = GraphCommandTools.CompareMap(_map, workMap);

        compareMap.Count.Should().Be(1);
        var index = compareMap.ToCursor();

        index.NextValue().Return().Cast<GraphNode>().Action(x =>
        {
            x.Key.Should().Be("node3");
            x.Tags.ToString().Should().Be("lang=java");
        });

        commandResults.Items.Count.Should().Be(1);
        var resultIndex = commandResults.Items.ToCursor();

        resultIndex.NextValue().Return().Action(x =>
        {
            x.CommandType.Should().Be(CommandType.UpdateNode);
            x.Status.IsOk().Should().BeTrue();
            x.Items.NotNull().Count.Should().Be(1);

            var resultIndex = x.Items.NotNull().ToCursor();
            resultIndex.NextValue().Return().Cast<GraphNode>().Action(x =>
            {
                x.Key.Should().Be("node3");
                x.Tags.ToString().Should().Be("lang=java,name=lop");
            });
        });
    }

    [Fact]
    public void SingleUpdateForEdge()
    {
        var workMap = _map.Copy();
        var newMapOption = workMap.Execute("update [fromKey=node1, toKey=node3] set -knows;", NullScopeContext.Instance);
        newMapOption.IsOk().Should().BeTrue();

        GraphQueryResults commandResults = newMapOption.Return();
        var compareMap = GraphCommandTools.CompareMap(_map, workMap);

        compareMap.Count.Should().Be(1);
        var index = compareMap.ToCursor();

        index.NextValue().Return().Cast<GraphEdge>().Action(x =>
        {
            x.FromKey.Should().Be("node1");
            x.ToKey.Should().Be("node3");
            x.EdgeType.Should().Be("default");
            x.Tags.ToString().Should().Be("level=1");
        });

        commandResults.Items.Count.Should().Be(1);
        var resultIndex = commandResults.Items.ToCursor();

        resultIndex.NextValue().Return().Action(x =>
        {
            x.CommandType.Should().Be(CommandType.UpdateEdge);
            x.Status.IsOk().Should().BeTrue();
            x.Items.NotNull().Count.Should().Be(1);

            var resultIndex = x.Items.NotNull().ToCursor();
            resultIndex.NextValue().Return().Cast<GraphEdge>().Action(x =>
            {
                x.FromKey.Should().Be("node1");
                x.ToKey.Should().Be("node3");
                x.EdgeType.Should().Be("default");
                x.Tags.ToString().Should().Be("level=1");
            });
        });
    }

    [Fact]
    public void SingleRemoveTagForEdge()
    {
        var workMap = _map.Copy();
        var newMapOption = workMap.Execute("update [fromKey=node4, toKey=node5] set t1;", NullScopeContext.Instance);
        newMapOption.IsOk().Should().BeTrue();

        GraphQueryResults commandResults = newMapOption.Return();
        var compareMap = GraphCommandTools.CompareMap(_map, workMap);

        compareMap.Count.Should().Be(1);
        var index = compareMap.ToCursor();

        index.NextValue().Return().Cast<GraphEdge>().Action(x =>
        {
            x.FromKey.Should().Be("node4");
            x.ToKey.Should().Be("node5");
            x.EdgeType.Should().Be("default");
            x.Tags.ToString().Should().Be("created,t1");
        });

        commandResults.Items.Count.Should().Be(1);
        var resultIndex = commandResults.Items.ToCursor();

        resultIndex.NextValue().Return().Action(x =>
        {
            x.CommandType.Should().Be(CommandType.UpdateEdge);
            x.Status.IsOk().Should().BeTrue();
            x.Items.NotNull().Count.Should().Be(1);

            var resultIndex = x.Items.NotNull().ToCursor();
            resultIndex.NextValue().Return().Cast<GraphEdge>().Action(x =>
            {
                x.FromKey.Should().Be("node4");
                x.ToKey.Should().Be("node5");
                x.EdgeType.Should().Be("default");
                x.Tags.ToString().Should().Be("created,t1");
            });
        });
    }
}
