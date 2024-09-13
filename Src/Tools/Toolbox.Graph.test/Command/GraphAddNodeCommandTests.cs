//using FluentAssertions;
//using Microsoft.Extensions.DependencyInjection;
//using Toolbox.Extensions;
//using Toolbox.Types;

//namespace Toolbox.Graph.test.Command;

//public class GraphAddNodeCommandTests
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

//        new GraphEdge("node1", "node2", tags: "knows,level=1"),
//        new GraphEdge("node1", "node3", tags: "knows,level=1"),
//        new GraphEdge("node6", "node3", tags: "created"),
//        new GraphEdge("node4", "node5", tags: "created"),
//        new GraphEdge("node4", "node3", tags: "created"),
//    };

//    [Fact]
//    public async Task SingleAddForNode()
//    {
//        var copyMap = _map.Clone();
//        var testClient = GraphTestStartup.CreateGraphTestHost(copyMap);
//        GraphMap map = testClient.ServiceProvider.GetRequiredService<GraphMap>();

//        var newMapOption = await testClient.ExecuteBatch("add node key=node99, newTags;", NullScopeContext.Instance);
//        newMapOption.IsOk().Should().BeTrue();
//        map.Nodes.Count.Should().Be(8);
//        map.Edges.Count.Should().Be(5);

//        GraphQueryResults commandResults = newMapOption.Return();
//        var compareMap = GraphCommandTools.CompareMap(_map, copyMap);

//        compareMap.Count.Should().Be(1);
//        compareMap[0].Cast<GraphNode>().Action(x =>
//        {
//            x.Key.Should().Be("node99");
//            x.Tags.ToTagsString().Should().Be("newTags");
//        });

//        commandResults.Items.Length.Should().Be(1);
//        var resultIndex = commandResults.Items.ToCursor();

//        resultIndex.NextValue().Return().Action(x =>
//        {
//            x.CommandType.Should().Be(CommandType.AddNode);
//            x.Status.IsOk().Should().BeTrue();
//            x.Status.Error.Should().BeNull();
//            x.Items.Should().NotBeNull();
//        });

//        var searchOption = await testClient.Execute("select (key=node99);", NullScopeContext.Instance);
//        searchOption.IsOk().Should().BeTrue();

//        GraphQueryResult search = searchOption.Return();
//        search.Status.IsOk().Should().BeTrue();
//        search.Items.Count.Should().Be(1);
//        search.Items[0].Cast<GraphNode>().Action(x =>
//        {
//            x.Key.Should().Be("node99");
//            x.Tags.Count.Should().Be(1);
//            x.TagsString.Should().Be("newTags");
//            x.DataMap.Count.Should().Be(0);
//        });

//        var deleteOption = await testClient.ExecuteBatch("delete (key=node99);", NullScopeContext.Instance);
//        deleteOption.IsOk().Should().BeTrue();
//        map.Nodes.Count.Should().Be(7);
//        map.Edges.Count.Should().Be(5);

//        searchOption = await testClient.Execute("select (key=node99);", NullScopeContext.Instance);
//        searchOption.IsOk().Should().BeTrue();
//        searchOption.Return().Items.Count.Should().Be(0);
//    }

//    [Fact]
//    public async Task SingleAddForNodeWithData()
//    {
//        var copyMap = _map.Clone();
//        var testClient = GraphTestStartup.CreateGraphTestHost(copyMap);
//        var newMapOption = await testClient.ExecuteBatch("add node key=node99, entity { 'aGVsbG8=' };", NullScopeContext.Instance);
//        newMapOption.IsOk().Should().BeTrue(newMapOption.ToString());

//        GraphQueryResults commandResults = newMapOption.Return();
//        var compareMap = GraphCommandTools.CompareMap(_map, copyMap);

//        compareMap.Count.Should().Be(1);
//        compareMap[0].Cast<GraphNode>().Action(x =>
//        {
//            x.Key.Should().Be("node99");
//            x.Tags.Count.Should().Be(0);

//            x.DataMap.Count.Should().Be(1);
//            x.DataMap.Action(y =>
//            {
//                y.TryGetValue("entity", out var entity).Should().BeTrue();
//                entity!.Validate().IsOk().Should().BeTrue();
//                entity!.FileId.Should().Be("nodes/node99/node99___entity.json");
//            });
//        });

//        var searchOption = await testClient.Execute("select (key=node99);", NullScopeContext.Instance);
//        searchOption.IsOk().Should().BeTrue();

//        GraphQueryResult search = searchOption.Return();
//        search.Status.IsOk().Should().BeTrue();
//        search.Items.Count.Should().Be(1);
//        search.Items[0].Cast<GraphNode>().Action(x =>
//        {
//            x.Key.Should().Be("node99");
//            x.Tags.Count.Should().Be(0);
//            x.DataMap.Count.Should().Be(1);
//            x.DataMap.Values.First().Action(y =>
//            {
//                y.FileId.Should().Be("nodes/node99/node99___entity.json");
//            });
//        });
//    }

//    [Fact]
//    public async Task SingleAddForNodeWithTagsCommand()
//    {
//        var copyMap = _map.Clone();
//        var testClient = GraphTestStartup.CreateGraphTestHost(copyMap);
//        var newMapOption = await testClient.ExecuteBatch("add node key=node99, -newTags;", NullScopeContext.Instance);
//        newMapOption.IsOk().Should().BeTrue();

//        GraphQueryResults commandResults = newMapOption.Return();
//        var compareMap = GraphCommandTools.CompareMap(_map, copyMap);

//        compareMap.Count.Should().Be(1);
//        compareMap[0].Cast<GraphNode>().Action(x =>
//        {
//            x.Key.Should().Be("node99");
//            x.Tags.Count.Should().Be(0);
//        });

//        commandResults.Items.Length.Should().Be(1);
//        var resultIndex = commandResults.Items.ToCursor();

//        resultIndex.NextValue().Return().Action(x =>
//        {
//            x.CommandType.Should().Be(CommandType.AddNode);
//            x.Status.IsOk().Should().BeTrue();
//            x.Status.Error.Should().BeNull();
//            x.Items.Should().NotBeNull();
//        });
//    }

//    [Fact]
//    public async Task SingleAddForNodeWithLink()
//    {
//        var copyMap = _map.Clone();
//        var testClient = GraphTestStartup.CreateGraphTestHost(copyMap);
//        var newMapOption = await testClient.ExecuteBatch("add node key=node99, link=ab/cd/ef;", NullScopeContext.Instance);
//        newMapOption.IsOk().Should().BeTrue();

//        GraphQueryResults commandResults = newMapOption.Return();
//        var compareMap = GraphCommandTools.CompareMap(_map, copyMap);

//        compareMap.Count.Should().Be(1);
//        compareMap[0].Cast<GraphNode>().Action(x =>
//        {
//            x.Key.Should().Be("node99");
//            x.Tags.ToTagsString().Should().Be("link=ab/cd/ef");
//        });

//        commandResults.Items.Length.Should().Be(1);
//        var resultIndex = commandResults.Items.ToCursor();

//        resultIndex.NextValue().Return().Action(x =>
//        {
//            x.CommandType.Should().Be(CommandType.AddNode);
//            x.Status.IsOk().Should().BeTrue();
//            x.Status.Error.Should().BeNull();
//            x.Items.Should().NotBeNull();
//        });
//    }

//    [Fact]
//    public async Task SingleAddForNodeWithMultipleTags()
//    {
//        var copyMap = _map.Clone();
//        var testClient = GraphTestStartup.CreateGraphTestHost(copyMap);
//        var newMapOption = await testClient.ExecuteBatch("add node key=node99, newTags,label=client;", NullScopeContext.Instance);
//        newMapOption.IsOk().Should().BeTrue();

//        GraphQueryResults commandResults = newMapOption.Return();
//        var compareMap = GraphCommandTools.CompareMap(_map, copyMap);

//        compareMap.Count.Should().Be(1);
//        compareMap[0].Cast<GraphNode>().Action(x =>
//        {
//            x.Key.Should().Be("node99");
//            x.Tags.ToTagsString().Should().Be("label=client,newTags");
//        });

//        commandResults.Items.Length.Should().Be(1);
//        var resultIndex = commandResults.Items.ToCursor();

//        resultIndex.NextValue().Return().Action(x =>
//        {
//            x.CommandType.Should().Be(CommandType.AddNode);
//            x.Status.IsOk().Should().BeTrue();
//            x.Items.Should().NotBeNull();
//        });
//    }

//    [Fact]
//    public async Task AddNodeWithPayload()
//    {
//        var copyMap = _map.Clone();
//        var testClient = GraphTestStartup.CreateGraphTestHost(copyMap);

//        string cmd = "upsert node key=user:44b5533a-31dd-463b-8712-2df9d8ee0780, AccessFailedCount=0,ConcurrencyStamp=3c03450e-0180-4281-8dbd-1f283e58a89e,Email='userName1@domain.com',EmailConfirmed=False,Id=44b5533a-31dd-463b-8712-2df9d8ee0780,LockoutEnabled=False,PhoneNumberConfirmed=False,SecurityStamp=b881724c-5f2b-4183-9b65-3faf75a1adf7,TwoFactorEnabled=False,UserName=userName1;";
//        var option = await testClient.ExecuteBatch(cmd, NullScopeContext.Instance);
//        option.IsOk().Should().BeTrue(option.ToString());
//    }
//}
