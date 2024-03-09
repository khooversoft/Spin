using FluentAssertions;
using Toolbox.Extensions;
using Toolbox.Types;

namespace Toolbox.Graph.test.Command;

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
        var newMapOption = _map.Execute("add node key=node99, newTags;", NullScopeContext.Instance);
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
        var newMapOption = _map.Execute("add node key=node99, newTags,label=client;", NullScopeContext.Instance);
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
        var newMapOption = _map.Execute("add edge fromKey=node7, toKey=node1, edgeType=newEdgeType, newTags;", NullScopeContext.Instance);
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

    [Fact]
    public void SingleUniqueAddForEdge()
    {
        var copyMap = _map.Copy();
        var newMapOption = _map.Execute("add unique edge fromKey=node7, toKey=node1, edgeType=newEdgeType, newTags;", NullScopeContext.Instance);
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

    [Fact]
    public void UniqueAddForEdgeWithExistingEdge()
    {
        var copyMap = _map.Copy();
        var newMapOption = _map.Execute("add unique edge fromKey=node4, toKey=node5;", NullScopeContext.Instance);
        newMapOption.IsError().Should().BeTrue();

        GraphQueryResults commandResults = newMapOption.Return();
        var compareMap = GraphCommandTools.CompareMap(copyMap, _map);

        compareMap.Count.Should().Be(0);

        commandResults.Items.Count.Should().Be(1);
        var resultIndex = commandResults.Items.ToCursor();

        resultIndex.NextValue().Return().Action(x =>
        {
            x.CommandType.Should().Be(CommandType.AddEdge);
            x.StatusCode.Should().Be(StatusCode.Conflict);
            x.Error.Should().BeNull();
            x.Items.Should().NotBeNull();
            x.Items.Count.Should().Be(0);
        });
    }

    [Fact]
    public void AddNodeWithPayload()
    {
        string cmd = "upsert node key=user:44b5533a-31dd-463b-8712-2df9d8ee0780, AccessFailedCount=0,ConcurrencyStamp=3c03450e-0180-4281-8dbd-1f283e58a89e,Email='userName1@domain.com',EmailConfirmed=False,Id=44b5533a-31dd-463b-8712-2df9d8ee0780,LockoutEnabled=False,PhoneNumberConfirmed=False,SecurityStamp=b881724c-5f2b-4183-9b65-3faf75a1adf7,TwoFactorEnabled=False,UserName=userName1;";
        var option = _map.Execute(cmd, NullScopeContext.Instance);
        option.IsOk().Should().BeTrue(option.ToString());
    }
}
