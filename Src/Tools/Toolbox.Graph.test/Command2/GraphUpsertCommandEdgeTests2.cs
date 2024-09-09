//using FluentAssertions;
//using Toolbox.Extensions;
//using Toolbox.Types;

//namespace Toolbox.Graph.test.Command;

//public class GraphUpsertCommandEdgeTests2
//{
//    private readonly GraphMap _map = new GraphMap()
//    {
//        new GraphNode("node1", tags: "name=marko,age=29"),
//        new GraphNode("node2", tags: "name=vadas,age=27"),
//        new GraphNode("node3", tags: "name=lop,lang=java"),
//        new GraphNode("node4", tags: "name=josh,age=32"),
//        new GraphNode("node5", tags: "name=ripple,lang=java"),
//        new GraphNode("node6", tags: "name=peter,age=35"),
//        new GraphNode("node7", tags: "lang=java"),

//        new GraphEdge("node1", "node2", edgeType: "et1", tags: "knows,level=1"),
//        new GraphEdge("node1", "node3", edgeType: "et1", tags: "knows,level=1"),
//        new GraphEdge("node6", "node3", edgeType: "et1", tags: "created"),
//        new GraphEdge("node4", "node5", edgeType: "et1", tags: "created"),
//        new GraphEdge("node4", "node3", edgeType: "et1", tags: "created"),
//    };

//    [Fact]
//    public async Task UpsertForEdge()
//    {
//        var copyMap = _map.Clone();
//        var testClient = GraphTestStartup.CreateGraphTestHost(copyMap);
//        var newMapOption = await testClient.ExecuteBatch("upsert edge fromKey=node6, toKey=node3, edgeType=et1, newTags;", NullScopeContext.Instance);
//        newMapOption.IsOk().Should().BeTrue(newMapOption.ToString());
//        copyMap.Edges.Count.Should().Be(_map.Edges.Count);

//        GraphQueryResults commandResults = newMapOption.Return();
//        var compareMap = GraphCommandTools.CompareMap(_map, copyMap);

//        compareMap.Count.Should().Be(1);
//        compareMap[0].Cast<GraphEdge>().Action(x =>
//        {
//            x.FromKey.Should().Be("node6");
//            x.ToKey.Should().Be("node3");
//            x.EdgeType.Should().Be("et1");
//            x.Tags.ToTagsString().Should().Be("created,newTags");
//        });

//        commandResults.Items.Length.Should().Be(1);
//        var resultIndex = commandResults.Items.ToCursor();

//        resultIndex.NextValue().Return().Action(x =>
//        {
//            x.CommandType.Should().Be(CommandType.AddEdge);
//            x.Status.IsOk().Should().BeTrue();
//            x.Items.Should().NotBeNull();
//        });
//    }

//    [Fact]
//    public async Task UpsertUniqueForWithoutEdgeTypeEdge()
//    {
//        var copyMap = _map.Clone();
//        var testClient = GraphTestStartup.CreateGraphTestHost(copyMap);
//        var newMapOption = await testClient.ExecuteBatch("upsert unique edge fromKey=node6, toKey=node3, newTags;", NullScopeContext.Instance);
//        newMapOption.IsConflict().Should().BeTrue(newMapOption.ToString());
//        copyMap.Edges.Count.Should().Be(_map.Edges.Count);

//        GraphQueryResults commandResults = newMapOption.Return();
//        var compareMap = GraphCommandTools.CompareMap(_map, copyMap);
//        compareMap.Count.Should().Be(0);
//    }

//    [Fact]
//    public async Task UpsertUniqueWithSameEdgeTypeForEdge()
//    {
//        var copyMap = _map.Clone();
//        var testClient = GraphTestStartup.CreateGraphTestHost(copyMap);
//        var newMapOption = await testClient.ExecuteBatch("upsert unique edge fromKey=node6, toKey=node3, edgeType=et1, newTags;", NullScopeContext.Instance);
//        newMapOption.IsConflict().Should().BeTrue(newMapOption.ToString());
//        copyMap.Edges.Count.Should().Be(_map.Edges.Count);

//        GraphQueryResults commandResults = newMapOption.Return();
//        var compareMap = GraphCommandTools.CompareMap(_map, copyMap);
//        compareMap.Count.Should().Be(0);
//    }

//    [Fact]
//    public async Task UpsertUniqueWithDifferentEdgeTypeForEdge()
//    {
//        var copyMap = _map.Clone();
//        var testClient = GraphTestStartup.CreateGraphTestHost(copyMap);
//        var newMapOption = await testClient.ExecuteBatch("upsert unique edge fromKey=node6, toKey=node3, edgeType=et2, newTags;", NullScopeContext.Instance);
//        newMapOption.IsOk().Should().BeTrue(newMapOption.ToString());
//        copyMap.Edges.Count.Should().Be(_map.Edges.Count + 1);

//        GraphQueryResults commandResults = newMapOption.Return();
//        var compareMap = GraphCommandTools.CompareMap(_map, copyMap);
//        compareMap.Count.Should().Be(1);

//        compareMap[0].Cast<GraphEdge>().Action(x =>
//        {
//            x.FromKey.Should().Be("node6");
//            x.ToKey.Should().Be("node3");
//            x.EdgeType.Should().Be("et2");
//            x.Tags.ToTagsString().Should().Be("newTags");
//        });
//    }

//    [Fact]
//    public async Task UpsertForEdgeWithRemoveTag()
//    {
//        var copyMap = _map.Clone();
//        var testClient = GraphTestStartup.CreateGraphTestHost(copyMap);
//        var newMapOption = await testClient.ExecuteBatch("upsert edge fromKey=node1, toKey=node2, edgeType=et1, -knows;", NullScopeContext.Instance);
//        newMapOption.IsOk().Should().BeTrue(newMapOption.ToString());
//        copyMap.Edges.Count.Should().Be(_map.Edges.Count);

//        GraphQueryResults commandResults = newMapOption.Return();
//        var compareMap = GraphCommandTools.CompareMap(_map, copyMap);

//        compareMap.Count.Should().Be(1);
//        compareMap[0].Cast<GraphEdge>().Action(x =>
//        {
//            x.FromKey.Should().Be("node1");
//            x.ToKey.Should().Be("node2");
//            x.EdgeType.Should().Be("et1");
//            x.Tags.ToTagsString().Should().Be("level=1");
//        });

//        commandResults.Items.Length.Should().Be(1);
//        var resultIndex = commandResults.Items.ToCursor();

//        resultIndex.NextValue().Return().Action(x =>
//        {
//            x.CommandType.Should().Be(CommandType.AddEdge);
//            x.Status.IsOk().Should().BeTrue();
//            x.Items.Should().NotBeNull();
//        });
//    }

//    [Fact]
//    public async Task SingleAddWithUpsertForEdge()
//    {
//        var copyMap = _map.Clone();
//        int currentEdgeCount = copyMap.Edges.Count;
//        var testClient = GraphTestStartup.CreateGraphTestHost(copyMap);
//        var newMapOption = await testClient.ExecuteBatch("upsert edge fromKey=node7, toKey=node1, edgeType=newEdgeType, newTags;", NullScopeContext.Instance);
//        newMapOption.IsOk().Should().BeTrue(newMapOption.ToString());
//        copyMap.Edges.Count.Should().Be(_map.Edges.Count + 1);

//        GraphQueryResults commandResults = newMapOption.Return();
//        var compareMap = GraphCommandTools.CompareMap(copyMap, _map);

//        compareMap.Count.Should().Be(1);
//        compareMap[0].Cast<GraphEdge>().Action(x =>
//        {
//            x.FromKey.Should().Be("node7");
//            x.ToKey.Should().Be("node1");
//            x.EdgeType.Should().Be("newEdgeType");
//            x.Tags.ToTagsString().Should().Be("newTags");
//        });

//        commandResults.Items.Length.Should().Be(1);
//        var resultIndex = commandResults.Items.ToCursor();

//        resultIndex.NextValue().Return().Action(x =>
//        {
//            x.CommandType.Should().Be(CommandType.AddEdge);
//            x.Status.IsOk().Should().BeTrue();
//            x.Items.Should().NotBeNull();
//        });
//    }
//}
