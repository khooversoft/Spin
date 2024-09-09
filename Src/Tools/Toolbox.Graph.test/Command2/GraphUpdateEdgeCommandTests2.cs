//using FluentAssertions;
//using Toolbox.Extensions;
//using Toolbox.Tools;
//using Toolbox.Types;

//namespace Toolbox.Graph.test.Command;

//public class GraphUpdateEdgeCommandTests2
//{
//    private static readonly GraphMap _map = new GraphMap()
//    {
//        new GraphNode("node1", tags: "name=marko,age=29"),
//        new GraphNode("node2", tags: "name=vadas,age=35"),
//        new GraphNode("node3", tags: "name=lop,lang=java"),
//        new GraphNode("node4", tags: "name=josh,age=32"),
//        new GraphNode("node5", tags: "name=ripple,lang=java"),
//        new GraphNode("node6", tags: "name=peter,age=35"),
//        new GraphNode("node7", tags: "lang=java"),

//        new GraphEdge("node1", "node2", tags: "knows,level=1"),
//        new GraphEdge("node1", "node3", tags: "knows,level=1"),
//        new GraphEdge("node6", "node3", tags: "created"),
//        new GraphEdge("node4", "node5", tags: "created"),
//        new GraphEdge("node4", "node3", tags: "created"),
//    };

//    [Fact]
//    public async Task SingleUpdateForEdge()
//    {
//        var workMap = _map.Clone();
//        var testClient = GraphTestStartup.CreateGraphTestHost(workMap);
//        var newMapOption = await testClient.ExecuteBatch("update [fromKey=node1, toKey=node3] set -knows;", NullScopeContext.Instance);
//        newMapOption.IsOk().Should().BeTrue();

//        GraphQueryResults commandResults = newMapOption.Return();
//        var compareMap = GraphCommandTools.CompareMap(_map, workMap);

//        compareMap.Count.Should().Be(1);
//        var index = compareMap.ToCursor();

//        index.NextValue().Return().Cast<GraphEdge>().Action(x =>
//        {
//            x.FromKey.Should().Be("node1");
//            x.ToKey.Should().Be("node3");
//            x.EdgeType.Should().Be("default");
//            x.Tags.ToTagsString().Should().Be("level=1");
//        });

//        commandResults.Items.Length.Should().Be(1);
//        var resultIndex = commandResults.Items.ToCursor();

//        resultIndex.NextValue().Return().Action(x =>
//        {
//            x.CommandType.Should().Be(CommandType.UpdateEdge);
//            x.Status.IsOk().Should().BeTrue();
//            x.Items.NotNull().Count.Should().Be(1);

//            var resultIndex = x.Items.NotNull().ToCursor();
//            resultIndex.NextValue().Return().Cast<GraphEdge>().Action(x =>
//            {
//                x.FromKey.Should().Be("node1");
//                x.ToKey.Should().Be("node3");
//                x.EdgeType.Should().Be("default");
//                x.Tags.ToTagsString().Should().Be("knows,level=1");
//            });
//        });
//    }

//    [Fact]
//    public async Task SingleRemoveTagForEdge()
//    {
//        var workMap = _map.Clone();
//        var testClient = GraphTestStartup.CreateGraphTestHost(workMap);
//        var newMapOption = await testClient.ExecuteBatch("update [fromKey=node4, toKey=node5] set t1;", NullScopeContext.Instance);
//        newMapOption.IsOk().Should().BeTrue();

//        GraphQueryResults commandResults = newMapOption.Return();
//        var compareMap = GraphCommandTools.CompareMap(_map, workMap);

//        compareMap.Count.Should().Be(1);
//        var index = compareMap.ToCursor();

//        index.NextValue().Return().Cast<GraphEdge>().Action(x =>
//        {
//            x.FromKey.Should().Be("node4");
//            x.ToKey.Should().Be("node5");
//            x.EdgeType.Should().Be("default");
//            x.Tags.ToTagsString().Should().Be("created,t1");
//        });

//        commandResults.Items.Length.Should().Be(1);
//        var resultIndex = commandResults.Items.ToCursor();

//        resultIndex.NextValue().Return().Action(x =>
//        {
//            x.CommandType.Should().Be(CommandType.UpdateEdge);
//            x.Status.IsOk().Should().BeTrue();
//            x.Items.NotNull().Count.Should().Be(1);

//            var resultIndex = x.Items.NotNull().ToCursor();
//            resultIndex.NextValue().Return().Cast<GraphEdge>().Action(x =>
//            {
//                x.FromKey.Should().Be("node4");
//                x.ToKey.Should().Be("node5");
//                x.EdgeType.Should().Be("default");
//                x.Tags.ToTagsString().Should().Be("created");
//            });
//        });
//    }
//}
