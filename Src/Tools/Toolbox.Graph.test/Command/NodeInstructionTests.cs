using FluentAssertions;
using Toolbox.Extensions;
using Toolbox.Types;

namespace Toolbox.Graph.test.Command;

public class NodeInstructionTests
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
    [InlineData("add node NodeKey=node4, toKey=node5;")]
    [InlineData("delete node from=node1, to=node2, type=et1 set newTags, t2=v2")]
    public async Task Failures(string query)
    {
        var copyMap = _map.Clone();
        var testClient = GraphTestStartup.CreateGraphTestHost(copyMap);
        var newMapOption = await testClient.ExecuteBatch(query, NullScopeContext.Instance);
        newMapOption.IsError().Should().BeTrue();

        copyMap.Nodes.Count.Should().Be(7);
        copyMap.Edges.Count.Should().Be(5);
    }

    [Fact]
    public async Task AddNode()
    {
        var copyMap = _map.Clone();
        var testClient = GraphTestStartup.CreateGraphTestHost(copyMap);
        var newMapOption = await testClient.ExecuteBatch("add node key=node8;", NullScopeContext.Instance);
        newMapOption.IsOk().Should().BeTrue(newMapOption.ToString());

        QueryBatchResult commandResults = newMapOption.Return();
        var compareMap = GraphCommandTools.CompareMap(_map, copyMap);

        compareMap.Count.Should().Be(1);
        compareMap[0].Cast<GraphNode>().Action(x =>
        {
            x.Key.Should().Be("node8");
            x.Tags.Count.Should().Be(0);
        });

        commandResults.Items.Count.Should().Be(1);
    }

    [Fact]
    public async Task AddNodeWithTag()
    {
        var copyMap = _map.Clone();
        var testClient = GraphTestStartup.CreateGraphTestHost(copyMap);
        var newMapOption = await testClient.ExecuteBatch("add node key=node9 set newTags;", NullScopeContext.Instance);
        newMapOption.IsOk().Should().BeTrue(newMapOption.ToString());

        QueryBatchResult commandResults = newMapOption.Return();
        commandResults.Items.Count.Should().Be(1);

        var compareMap = GraphCommandTools.CompareMap(_map, copyMap);

        compareMap.Count.Should().Be(1);
        compareMap[0].Cast<GraphNode>().Action(x =>
        {
            x.Key.Should().Be("node9");
            x.Tags.ToTagsString().Should().Be("newTags");
        });
    }

    [Fact]
    public async Task AddNodeWithRemoveTagsThatDoesNotExist()
    {
        var copyMap = _map.Clone();
        var testClient = GraphTestStartup.CreateGraphTestHost(copyMap);
        var newMapOption = await testClient.ExecuteBatch("add node key=node10 set -newTags;", NullScopeContext.Instance);
        newMapOption.IsOk().Should().BeTrue();

        QueryBatchResult commandResults = newMapOption.Return();
        commandResults.Items.Count.Should().Be(1);

        var compareMap = GraphCommandTools.CompareMap(_map, copyMap);

        compareMap.Count.Should().Be(1);
        compareMap[0].Cast<GraphNode>().Action(x =>
        {
            x.Key.Should().Be("node10");
            x.Tags.Count.Should().Be(0);
        });
    }

    [Fact]
    public async Task AddNodeWithTwoTags()
    {
        var copyMap = _map.Clone();
        var testClient = GraphTestStartup.CreateGraphTestHost(copyMap);
        var newMapOption = await testClient.ExecuteBatch("add node key=node8 set newTags, t2=v2;", NullScopeContext.Instance);
        newMapOption.IsOk().Should().BeTrue();

        QueryBatchResult commandResults = newMapOption.Return();
        var compareMap = GraphCommandTools.CompareMap(_map, copyMap);

        compareMap.Count.Should().Be(1);
        compareMap[0].Cast<GraphNode>().Action(x =>
        {
            x.Key.Should().Be("node8");
            x.Tags.ToTagsString().Should().Be("newTags,t2=v2");
        });

        commandResults.Items.Count.Should().Be(1);
    }

    [Fact]
    public async Task DeleteNode()
    {
        var copyMap = _map.Clone();
        var testClient = GraphTestStartup.CreateGraphTestHost(copyMap);
        var newMapOption = await testClient.ExecuteBatch("delete node key=node1 ;", NullScopeContext.Instance);
        newMapOption.IsOk().Should().BeTrue(newMapOption.ToString());

        QueryBatchResult commandResults = newMapOption.Return();
        var compareMap = GraphCommandTools.CompareMap(_map, copyMap);

        compareMap.Count.Should().Be(3);
        compareMap.OfType<GraphNode>().First().Action(x =>
        {
            x.Key.Should().Be("node1");
            x.Tags.ToTagsString().Should().Be("age=29,name=marko");
        });
        compareMap.OfType<GraphEdge>().OrderBy(x => x.ToKey).ToArray().Action(x =>
        {
            x.Length.Should().Be(2);
            x[0].Action(y =>
            {
                y.FromKey.Should().Be("node1");
                y.ToKey.Should().Be("node2");
                y.Tags.ToTagsString().Should().Be("knows,level=1");
            });
            x[1].Action(y =>
            {
                y.FromKey.Should().Be("node1");
                y.ToKey.Should().Be("node3");
                y.Tags.ToTagsString().Should().Be("knows,level=1");
            });
        });

        commandResults.Items.Count.Should().Be(1);
    }

    [Fact]
    public async Task DeleteNodeIfExist()
    {
        var copyMap = _map.Clone();
        var testClient = GraphTestStartup.CreateGraphTestHost(copyMap);

        // Verify delete will fail
        var newMapOption = await testClient.ExecuteBatch("delete node key=node11 ;", NullScopeContext.Instance);
        newMapOption.IsError().Should().BeTrue(newMapOption.ToString());

        // Delet should not fail because of 'ifexist'
        newMapOption = await testClient.ExecuteBatch("delete node ifexist key=node111 ;", NullScopeContext.Instance);
        newMapOption.IsOk().Should().BeTrue(newMapOption.ToString());

        QueryBatchResult commandResults = newMapOption.Return();
        commandResults.Items.Count.Should().Be(1);

        var compareMap = GraphCommandTools.CompareMap(_map, copyMap);
        compareMap.Count.Should().Be(0);
    }

    [Fact]
    public async Task SetNode()
    {
        var copyMap = _map.Clone();
        var testClient = GraphTestStartup.CreateGraphTestHost(copyMap);
        var newMapOption = await testClient.ExecuteBatch("set node key=node8 ;", NullScopeContext.Instance);
        newMapOption.IsOk().Should().BeTrue(newMapOption.ToString());

        QueryBatchResult commandResults = newMapOption.Return();
        var compareMap = GraphCommandTools.CompareMap(_map, copyMap);

        compareMap.Count.Should().Be(1);
        compareMap[0].Cast<GraphNode>().Action(x =>
        {
            x.Key.Should().Be("node8");
            x.Tags.Count.Should().Be(0);
        });

        commandResults.Items.Count.Should().Be(1);
    }

    [Fact]
    public async Task UpdateNodeTags()
    {
        var copyMap = _map.Clone();
        var testClient = GraphTestStartup.CreateGraphTestHost(copyMap);
        var newMapOption = await testClient.ExecuteBatch("set node key=node11 set t1, t2=v2 ;", NullScopeContext.Instance);
        newMapOption.IsOk().Should().BeTrue(newMapOption.ToString());

        QueryBatchResult commandResults = newMapOption.Return();
        var compareMap = GraphCommandTools.CompareMap(_map, copyMap);

        compareMap.Count.Should().Be(1);
        compareMap[0].Cast<GraphNode>().Action(x =>
        {
            x.Key.Should().Be("node11");
            x.Tags.ToTagsString().Should().Be("t1,t2=v2");
        });

        commandResults.Items.Count.Should().Be(1);
    }

    [Fact]
    public async Task SetNodeWithTags()
    {
        var copyMap = _map.Clone();
        var testClient = GraphTestStartup.CreateGraphTestHost(copyMap);
        var newMapOption = await testClient.ExecuteBatch("set node key=node4 set t1, t2=v2 ;", NullScopeContext.Instance);
        newMapOption.IsOk().Should().BeTrue(newMapOption.ToString());

        QueryBatchResult commandResults = newMapOption.Return();
        var compareMap = GraphCommandTools.CompareMap(_map, copyMap);

        compareMap.Count.Should().Be(1);
        compareMap[0].Cast<GraphNode>().Action(x =>
        {
            x.Key.Should().Be("node4");
            x.Tags.ToTagsString().Should().Be("age=32,name=josh,t1,t2=v2");
        });

        commandResults.Items.Count.Should().Be(1);
    }

    [Fact]
    public async Task SetNodeWithRemoveTag()
    {
        var copyMap = _map.Clone();
        var testClient = GraphTestStartup.CreateGraphTestHost(copyMap);
        var newMapOption = await testClient.ExecuteBatch("set node key=node4 set -age ;", NullScopeContext.Instance);
        newMapOption.IsOk().Should().BeTrue(newMapOption.ToString());

        QueryBatchResult commandResults = newMapOption.Return();
        var compareMap = GraphCommandTools.CompareMap(_map, copyMap);

        compareMap.Count.Should().Be(1);
        compareMap[0].Cast<GraphNode>().Action(x =>
        {
            x.Key.Should().Be("node4");
            x.Tags.ToTagsString().Should().Be("name=josh");
        });

        commandResults.Items.Count.Should().Be(1);
    }

    [Fact]
    public async Task SetNodeAsSchema()
    {
        var copyMap = _map.Clone();
        var testClient = GraphTestStartup.CreateGraphTestHost(copyMap);
        var newMapOption = await testClient.ExecuteBatch("set node key=provider:provider1/provider1-key set uniqueIndex;", NullScopeContext.Instance);
        newMapOption.IsOk().Should().BeTrue(newMapOption.ToString());

        QueryBatchResult commandResults = newMapOption.Return();
        var compareMap = GraphCommandTools.CompareMap(_map, copyMap);

        compareMap.Count.Should().Be(1);
        compareMap[0].Cast<GraphNode>().Action(x =>
        {
            x.Key.Should().Be("provider:provider1/provider1-key");
            x.Tags.ToTagsString().Should().Be("uniqueIndex");
        });

        commandResults.Items.Count.Should().Be(1);
    }
}
