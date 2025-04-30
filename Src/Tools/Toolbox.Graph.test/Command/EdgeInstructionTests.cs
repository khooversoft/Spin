using Microsoft.Extensions.DependencyInjection;
using Toolbox.Extensions;
using Toolbox.Graph.test.Application;
using Toolbox.Tools;
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
        using GraphHostService testClient = await GraphTestStartup.CreateGraphService(_map.Clone());

        var newMapOption = await testClient.ExecuteBatch(query, NullScopeContext.Default);
        newMapOption.IsError().BeTrue();
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
        fromLookup.OrderBy(x => x.ToString()).SequenceEqual(edges.OrderBy(x => x.ToString())).BeTrue();
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

        var toLookups = copyMap.Edges.LookupByToKey(["node3"]).Select(x => x);
        Enumerable.SequenceEqual(toLookups.OrderBy(x => x.ToString()), toLookups.OrderBy(x => x.ToString()), GraphEdgePrimaryKeyComparer.Default).BeTrue();
    }

    [Fact]
    public void LookupByEdgeTypes()
    {
        var copyMap = _map.Clone();

        GraphEdgePrimaryKey[] edges = [
            new GraphEdgePrimaryKey { FromKey = "node6", ToKey = "node3", EdgeType = "et2" },
            new GraphEdgePrimaryKey { FromKey = "node4", ToKey = "node5", EdgeType = "et2" },
            ];

        var edgeTypes = copyMap.Edges.LookupByEdgeType(["et2"]).Select(x => x);
        Enumerable.SequenceEqual(edgeTypes.OrderBy(x => x.ToString()), edges.OrderBy(x => x.ToString()), GraphEdgePrimaryKeyComparer.Default).BeTrue();
    }

    [Fact]
    public async Task AddEdge()
    {
        using GraphHostService testClient = await TestApplication.CreateTestGraphService(_map.Clone());
        var collector = testClient.Services.GetRequiredService<GraphMapCounter>();

        var newMapOption = await testClient.ExecuteBatch("add edge from=node7, to=node1, type=newEdgeType set newTags;", NullScopeContext.Default);
        newMapOption.IsOk().BeTrue();

        var pk = new GraphEdgePrimaryKey { FromKey = "node7", ToKey = "node1", EdgeType = "newEdgeType" };

        var fromLookup = testClient.Map.Edges.LookupByFromKey(["node7"]);
        Enumerable.SequenceEqual(fromLookup, [pk]).BeTrue();

        var toLookup = testClient.Map.Edges.LookupByToKey(["node1"]);
        Enumerable.SequenceEqual(toLookup, [pk]).BeTrue();

        var edgeType = testClient.Map.Edges.LookupByEdgeType(["newEdgeType"]);
        Enumerable.SequenceEqual(edgeType, [pk]).BeTrue();

        collector.Edges.Count.Value.Be(6);
        collector.Edges.Added.Value.Be(6);
        collector.Edges.Updated.Value.Be(0);
        collector.Edges.IndexHit.Value.Be(3);
        collector.Edges.IndexMissed.Value.Be(0);

        QueryBatchResult commandResults = newMapOption.Return();
        var compareMap = GraphCommandTools.CompareMap(_map, testClient.Map);

        compareMap.Count.Be(1);
        compareMap[0].Cast<GraphEdge>().Action(x =>
        {
            x.FromKey.Be("node7");
            x.ToKey.Be("node1");
            x.EdgeType.Be("newEdgeType");
            x.Tags.ToTagsString().Be("newTags");
        });

        commandResults.Items.Count.Be(1);
        var resultIndex = commandResults.Items.ToCursor();

        resultIndex.NextValue().Return().Action(x =>
        {
            x.Option.IsOk().BeTrue();
            x.Nodes.Count.Be(0);
            x.Edges.Count.Be(0);
            x.DataLinks.Count.Be(0);
        });
    }

    [Fact]
    public async Task AddEdgeWithRemoveTagCommandFilterOut()
    {
        using GraphHostService testClient = await GraphTestStartup.CreateGraphService(_map.Clone());

        var newMapOption = await testClient.ExecuteBatch("add edge from=node7, to=node1, type=newEdgeType set -newTags;", NullScopeContext.Default);
        newMapOption.IsOk().BeTrue();

        QueryBatchResult commandResults = newMapOption.Return();
        var compareMap = GraphCommandTools.CompareMap(_map, testClient.Map);

        compareMap.Count.Be(1);
        compareMap[0].Cast<GraphEdge>().Action(x =>
        {
            x.FromKey.Be("node7");
            x.ToKey.Be("node1");
            x.EdgeType.Be("newEdgeType");
            x.Tags.Count.Be(0);
        });

        commandResults.Items.Count.Be(1);
    }

    [Fact]
    public async Task AddEdgeWithTag()
    {
        using GraphHostService testClient = await GraphTestStartup.CreateGraphService(_map.Clone());

        var newMapOption = await testClient.ExecuteBatch("add edge from=node7, to=node1, type=newEdgeType set newTags;", NullScopeContext.Default);
        newMapOption.IsOk().BeTrue();

        QueryBatchResult commandResults = newMapOption.Return();
        var compareMap = GraphCommandTools.CompareMap(_map, testClient.Map);

        compareMap.Count.Be(1);
        compareMap[0].Cast<GraphEdge>().Action(x =>
        {
            x.FromKey.Be("node7");
            x.ToKey.Be("node1");
            x.EdgeType.Be("newEdgeType");
            x.Tags.ToTagsString().Be("newTags");
        });

        commandResults.Items.Count.Be(1);
    }

    [Fact]
    public async Task DeleteEdge()
    {
        using GraphHostService testClient = await GraphTestStartup.CreateGraphService(_map.Clone());

        var newMapOption = await testClient.ExecuteBatch("delete edge from=node1, to=node3, type=et1 ;", NullScopeContext.Default);
        newMapOption.IsOk().BeTrue();

        QueryBatchResult commandResults = newMapOption.Return();
        var compareMap = GraphCommandTools.CompareMap(_map, testClient.Map);

        compareMap.Count.Be(1);
        compareMap[0].Cast<GraphEdge>().Action(x =>
        {
            x.FromKey.Be("node1");
            x.ToKey.Be("node3");
            x.EdgeType.Be("et1");
            x.Tags.ToTagsString().Be("knows,level=1");
        });

        commandResults.Items.Count.Be(1);
    }

    [Fact]
    public async Task DeleteEdgeIfExist()
    {
        using GraphHostService testClient = await GraphTestStartup.CreateGraphService(_map.Clone());

        // Verify delete will fail
        var newMapOption = await testClient.ExecuteBatch("delete edge from=node7, to=node2, type=et1 ;", NullScopeContext.Default);
        newMapOption.IsError().BeTrue();

        // Delet should not fail because of 'ifexist'
        newMapOption = await testClient.ExecuteBatch("delete edge ifexist from=node7, to=node2, type=et1 ;", NullScopeContext.Default);
        newMapOption.IsOk().BeTrue();

        QueryBatchResult commandResults = newMapOption.Return();
        commandResults.Items.Count.Be(1);

        var compareMap = GraphCommandTools.CompareMap(_map, testClient.Map);
        compareMap.Count.Be(0);
    }

    [Fact]
    public async Task SetEdge()
    {
        using GraphHostService testClient = await GraphTestStartup.CreateGraphService(_map.Clone());

        var newMapOption = await testClient.ExecuteBatch("set edge from=node4, to=node3, type=et3 set t1, t2=v2 ;", NullScopeContext.Default);
        newMapOption.IsOk().BeTrue();

        QueryBatchResult commandResults = newMapOption.Return();
        var compareMap = GraphCommandTools.CompareMap(_map, testClient.Map);

        compareMap.Count.Be(1);
        compareMap[0].Cast<GraphEdge>().Action(x =>
        {
            x.FromKey.Be("node4");
            x.ToKey.Be("node3");
            x.EdgeType.Be("et3");
            x.Tags.ToTagsString().Be("created,t1,t2=v2");
        });

        commandResults.Items.Count.Be(1);
    }

    [Fact]
    public async Task SetEdgeRemoveTag()
    {
        using GraphHostService testClient = await GraphTestStartup.CreateGraphService(_map.Clone());

        var newMapOption = await testClient.ExecuteBatch("set edge from=node1, to=node2, type=et1 set t1, t2=v2, -knows ;", NullScopeContext.Default);
        newMapOption.IsOk().BeTrue();

        QueryBatchResult commandResults = newMapOption.Return();
        var compareMap = GraphCommandTools.CompareMap(_map, testClient.Map);

        compareMap.Count.Be(1);
        compareMap[0].Cast<GraphEdge>().Action(x =>
        {
            x.FromKey.Be("node1");
            x.ToKey.Be("node2");
            x.EdgeType.Be("et1");
            x.Tags.ToTagsString().Be("level=1,t1,t2=v2");
        });

        commandResults.Items.Count.Be(1);
    }
}
