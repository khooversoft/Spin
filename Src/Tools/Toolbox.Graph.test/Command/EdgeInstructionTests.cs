using FluentAssertions;
using Toolbox.Extensions;
using Toolbox.Types;

namespace Toolbox.Graph.test.Command;

public class EdgeInstructionTests
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
        new GraphEdge("node6", "node3", edgeType: "et2", tags: "created"),
        new GraphEdge("node4", "node5", edgeType: "et2", tags: "created"),
        new GraphEdge("node4", "node3", edgeType: "et3", tags: "created"),
    };

    [Theory]
    [InlineData("add edge from=node4, to=node5;")]
    [InlineData("add edge from=node4, to=node5, type=et2;")]
    public async Task Failures(string query)
    {
        var copyMap = _map.Clone();
        var testClient = GraphTestStartup.CreateGraphTestHost(copyMap);
        var newMapOption = await testClient.ExecuteBatch(query, NullScopeContext.Default);
        newMapOption.IsError().Should().BeTrue();
    }

    [Fact]
    public void LookupByFromKey()
    {
        var copyMap = _map.Clone();

        GraphEdgePrimaryKey[] edges = [
            new GraphEdgePrimaryKey { FromKey = "node4", ToKey = "node5", EdgeType = "et2" },
            new GraphEdgePrimaryKey { FromKey = "node4", ToKey = "node3", EdgeType = "et3" },
            ];

        var fromLookup = copyMap.Edges.LookupByFromKey(["node4"]);
        fromLookup.Should().BeEquivalentTo(edges);
    }

    [Fact]
    public void LookupByToKey()
    {
        var copyMap = _map.Clone();

        GraphEdgePrimaryKey[] edges = [
            new GraphEdgePrimaryKey { FromKey = "node1", ToKey = "node3", EdgeType = "et1" },
            new GraphEdgePrimaryKey { FromKey = "node6", ToKey = "node3", EdgeType = "et2" },
            new GraphEdgePrimaryKey { FromKey = "node4", ToKey = "node3", EdgeType = "et3" },
            ];

        var toLookups = copyMap.Edges.LookupByToKey(["node3"]).Select(x => x.GetPrimaryKey());
        Enumerable.SequenceEqual(toLookups.OrderBy(x => x.ToString()), toLookups.OrderBy(x => x.ToString()), GraphEdgePrimaryKeyComparer.Default).Should().BeTrue();
    }

    [Fact]
    public void LookupByEdgeTypes()
    {
        var copyMap = _map.Clone();

        GraphEdgePrimaryKey[] edges = [
            new GraphEdgePrimaryKey { FromKey = "node6", ToKey = "node3", EdgeType = "et2" },
            new GraphEdgePrimaryKey { FromKey = "node4", ToKey = "node5", EdgeType = "et2" },
            ];

        var edgeTypes = copyMap.Edges.LookupByEdgeType(["et2"]).Select(x => x.GetPrimaryKey());
        Enumerable.SequenceEqual(edgeTypes.OrderBy(x => x.ToString()), edges.OrderBy(x => x.ToString()), GraphEdgePrimaryKeyComparer.Default).Should().BeTrue();
    }

    [Fact]
    public async Task AddEdge()
    {
        var copyMap = _map.Clone();
        var testClient = GraphTestStartup.CreateGraphTestHost(copyMap);
        var newMapOption = await testClient.ExecuteBatch("add edge from=node7, to=node1, type=newEdgeType set newTags;", NullScopeContext.Default);
        newMapOption.IsOk().Should().BeTrue(newMapOption.ToString());


        var pk = new GraphEdgePrimaryKey { FromKey = "node7", ToKey = "node1", EdgeType = "newEdgeType" };

        var fromLookup = copyMap.Edges.LookupByFromKey(["node7"]);
        fromLookup.Should().BeEquivalentTo([pk]);

        var toLookup = copyMap.Edges.LookupByToKey(["node1"]);
        toLookup.Should().BeEquivalentTo([pk]);

        var edgeType = copyMap.Edges.LookupByEdgeType(["newEdgeType"]);
        toLookup.Should().BeEquivalentTo([pk]);

        copyMap.Meter.Edge.GetCount().Should().Be(6);
        copyMap.Meter.Edge.GetAdded().Should().Be(6);
        copyMap.Meter.Edge.GetUpdated().Should().Be(0);
        copyMap.Meter.Edge.GetIndexHit().Should().Be(3);
        copyMap.Meter.Edge.GetIndexMissed().Should().Be(0);

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
            x.Edges.Count.Should().Be(0);
            x.DataLinks.Count.Should().Be(0);
        });
    }

    [Fact]
    public async Task AddEdgeWithRemoveTagCommandFilterOut()
    {
        var copyMap = _map.Clone();
        var testClient = GraphTestStartup.CreateGraphTestHost(copyMap);
        var newMapOption = await testClient.ExecuteBatch("add edge from=node7, to=node1, type=newEdgeType set -newTags;", NullScopeContext.Default);
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
    }

    [Fact]
    public async Task AddEdgeWithTag()
    {
        var copyMap = _map.Clone();
        var testClient = GraphTestStartup.CreateGraphTestHost(copyMap);
        var newMapOption = await testClient.ExecuteBatch("add edge from=node7, to=node1, type=newEdgeType set newTags;", NullScopeContext.Default);
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
    }

    [Fact]
    public async Task DeleteEdge()
    {
        var copyMap = _map.Clone();
        var testClient = GraphTestStartup.CreateGraphTestHost(copyMap);
        var newMapOption = await testClient.ExecuteBatch("delete edge from=node1, to=node3, type=et1 ;", NullScopeContext.Default);
        newMapOption.IsOk().Should().BeTrue();

        QueryBatchResult commandResults = newMapOption.Return();
        var compareMap = GraphCommandTools.CompareMap(_map, copyMap);

        compareMap.Count.Should().Be(1);
        compareMap[0].Cast<GraphEdge>().Action(x =>
        {
            x.FromKey.Should().Be("node1");
            x.ToKey.Should().Be("node3");
            x.EdgeType.Should().Be("et1");
            x.Tags.ToTagsString().Should().Be("knows,level=1");
        });

        commandResults.Items.Count.Should().Be(1);
    }

    [Fact]
    public async Task DeleteEdgeIfExist()
    {
        var copyMap = _map.Clone();
        var testClient = GraphTestStartup.CreateGraphTestHost(copyMap);

        // Verify delete will fail
        var newMapOption = await testClient.ExecuteBatch("delete edge from=node7, to=node2, type=et1 ;", NullScopeContext.Default);
        newMapOption.IsError().Should().BeTrue(newMapOption.ToString());

        // Delet should not fail because of 'ifexist'
        newMapOption = await testClient.ExecuteBatch("delete edge ifexist from=node7, to=node2, type=et1 ;", NullScopeContext.Default);
        newMapOption.IsOk().Should().BeTrue(newMapOption.ToString());

        QueryBatchResult commandResults = newMapOption.Return();
        commandResults.Items.Count.Should().Be(1);

        var compareMap = GraphCommandTools.CompareMap(_map, copyMap);
        compareMap.Count.Should().Be(0);
    }

    [Fact]
    public async Task SetEdge()
    {
        var copyMap = _map.Clone();
        var testClient = GraphTestStartup.CreateGraphTestHost(copyMap);
        var newMapOption = await testClient.ExecuteBatch("set edge from=node4, to=node3, type=et3 set t1, t2=v2 ;", NullScopeContext.Default);
        newMapOption.IsOk().Should().BeTrue();

        QueryBatchResult commandResults = newMapOption.Return();
        var compareMap = GraphCommandTools.CompareMap(_map, copyMap);

        compareMap.Count.Should().Be(1);
        compareMap[0].Cast<GraphEdge>().Action(x =>
        {
            x.FromKey.Should().Be("node4");
            x.ToKey.Should().Be("node3");
            x.EdgeType.Should().Be("et3");
            x.Tags.ToTagsString().Should().Be("created,t1,t2=v2");
        });

        commandResults.Items.Count.Should().Be(1);
    }

    [Fact]
    public async Task SetEdgeRemoveTag()
    {
        var copyMap = _map.Clone();
        var testClient = GraphTestStartup.CreateGraphTestHost(copyMap);
        var newMapOption = await testClient.ExecuteBatch("set edge from=node1, to=node2, type=et1 set t1, t2=v2, -knows ;", NullScopeContext.Default);
        newMapOption.IsOk().Should().BeTrue();

        QueryBatchResult commandResults = newMapOption.Return();
        var compareMap = GraphCommandTools.CompareMap(_map, copyMap);

        compareMap.Count.Should().Be(1);
        compareMap[0].Cast<GraphEdge>().Action(x =>
        {
            x.FromKey.Should().Be("node1");
            x.ToKey.Should().Be("node2");
            x.EdgeType.Should().Be("et1");
            x.Tags.ToTagsString().Should().Be("level=1,t1,t2=v2");
        });

        commandResults.Items.Count.Should().Be(1);
    }
}
