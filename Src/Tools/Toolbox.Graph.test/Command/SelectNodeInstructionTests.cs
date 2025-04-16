using Microsoft.Extensions.DependencyInjection;
using Toolbox.Graph.test.Application;
using Toolbox.Tools.Should;
using Toolbox.Types;
using Xunit.Abstractions;

namespace Toolbox.Graph.test.Command;

public class SelectNodeInstructionTests
{
    private readonly GraphMap _map = new GraphMap()
    {
        new GraphNode("node1", tags: "name=marko,age=29"),
        new GraphNode("node2", tags: "name=vadas,age=27"),
        new GraphNode("node3", tags: "name=lop,lang=java;"),
        new GraphNode("node4", tags: "name=josh,age=32,user"),
        new GraphNode("node5", tags: "name=ripple,lang=java"),
        new GraphNode("node6", tags: "name=peter,age=35"),
        new GraphNode("node7", tags: "lang=java"),

        new GraphEdge("node1", "node2", edgeType: "et1", tags: "knows,level=1"),
        new GraphEdge("node1", "node3", edgeType: "et1", tags: "knows,level=1"),
        new GraphEdge("node6", "node3", edgeType: "et1", tags: "created"),
        new GraphEdge("node4", "node5", edgeType: "et1", tags: "created"),
        new GraphEdge("node4", "node3", edgeType : "et1", tags: "created"),
        new GraphEdge("node5", "node4", edgeType : "et1", tags: "created"),
    };

    private readonly ITestOutputHelper _outputHelper;
    public SelectNodeInstructionTests(ITestOutputHelper outputHelper) => _outputHelper = outputHelper;

    [Fact]
    public async Task SelectNode()
    {
        using GraphHostService testClient = await TestApplication.CreateTestGraphService(_map.Clone(), _outputHelper);
        var collector = testClient.Services.GetRequiredService<GraphMapCounter>();
        var context = testClient.CreateScopeContext<SelectNodeInstructionTests>();

        var newMapOption = await testClient.Execute("select (key=node2) a1 ;", context);
        newMapOption.IsOk().Should().BeTrue(newMapOption.ToString());

        QueryResult result = newMapOption.Return();
        result.Option.IsOk().Should().BeTrue();
        result.Alias.Should().Be("a1");
        result.Nodes.Count.Should().Be(1);
        result.Edges.Count.Should().Be(0);
        result.DataLinks.Count.Should().Be(0);

        result.Nodes[0].Key.Should().Be("node2");
        result.Nodes[0].Tags.ToTagsString().Should().Be("age=27,name=vadas");

        collector.Nodes.IndexHit.Value.Should().Be(1);
        collector.Nodes.IndexMissed.Value.Should().Be(0);
        collector.Nodes.IndexScan.Value.Should().Be(0);
    }

    [Fact]
    public async Task SelectNodeWithTag()
    {
        using GraphHostService testClient = await TestApplication.CreateTestGraphService(_map.Clone(), _outputHelper);
        var collector = testClient.Services.GetRequiredService<GraphMapCounter>();
        var context = testClient.CreateScopeContext<SelectNodeInstructionTests>();

        var newMapOption = await testClient.Execute("select (user) ;", context);
        newMapOption.IsOk().Should().BeTrue(newMapOption.ToString());

        QueryResult result = newMapOption.Return();
        result.Option.IsOk().Should().BeTrue();
        result.Alias.Should().NotBeEmpty();
        result.Nodes.Count.Should().Be(1);
        result.Edges.Count.Should().Be(0);
        result.DataLinks.Count.Should().Be(0);

        result.Nodes[0].Key.Should().Be("node4");
        result.Nodes[0].Tags.ToTagsString().Should().Be("age=32,name=josh,user");

        collector.Nodes.IndexHit.Value.Should().Be(1);
        collector.Nodes.IndexMissed.Value.Should().Be(0);
        collector.Nodes.IndexScan.Value.Should().Be(0);
    }

    [Fact]
    public async Task SelectAllNodes()
    {
        using GraphHostService testClient = await TestApplication.CreateTestGraphService(_map.Clone(), _outputHelper);
        var collector = testClient.Services.GetRequiredService<GraphMapCounter>();
        var context = testClient.CreateScopeContext<SelectNodeInstructionTests>();

        var newMapOption = await testClient.Execute("select (*) ;", context);
        newMapOption.IsOk().Should().BeTrue(newMapOption.ToString());

        QueryResult result = newMapOption.Return();
        result.Option.IsOk().Should().BeTrue();
        result.Alias.Should().NotBeEmpty();
        result.Nodes.Count.Should().Be(7);
        result.Edges.Count.Should().Be(0);
        result.DataLinks.Count.Should().Be(0);

        result.Nodes.Select(x => x.Key).OrderBy(x => x).SequenceEqual(["node1", "node2", "node3", "node4", "node5", "node6", "node7"]).Should().BeTrue();

        collector.Nodes.IndexHit.Value.Should().Be(0);
        collector.Nodes.IndexMissed.Value.Should().Be(0);
        collector.Nodes.IndexScan.Value.Should().Be(1);
    }

    [Fact]
    public async Task SelectAllNodesWithAgeTag()
    {
        using GraphHostService testClient = await TestApplication.CreateTestGraphService(_map.Clone(), _outputHelper);
        var collector = testClient.Services.GetRequiredService<GraphMapCounter>();
        var context = testClient.CreateScopeContext<SelectNodeInstructionTests>();

        var newMapOption = await testClient.Execute("select (age) ;", context);
        newMapOption.IsOk().Should().BeTrue(newMapOption.ToString());

        QueryResult result = newMapOption.Return();
        result.Option.IsOk().Should().BeTrue();
        result.Alias.Should().NotBeEmpty();
        result.Nodes.Count.Should().Be(4);
        result.Edges.Count.Should().Be(0);
        result.DataLinks.Count.Should().Be(0);

        result.Nodes.Select(x => x.Key).OrderBy(x => x).SequenceEqual(["node1", "node2", "node4", "node6"]).Should().BeTrue();

        collector.Nodes.IndexHit.Value.Should().Be(1);
        collector.Nodes.IndexMissed.Value.Should().Be(0);
        collector.Nodes.IndexScan.Value.Should().Be(0);
    }
}
