using Microsoft.Extensions.DependencyInjection;
using Toolbox.Graph.test.Application;
using Toolbox.Tools.Should;
using Toolbox.Types;
using Xunit.Abstractions;

namespace Toolbox.Graph.test.Command;

public class SelectEdgeInstructionTests
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
        new GraphEdge("node6", "node3", edgeType: "et3", tags: "created"),
        new GraphEdge("node4", "node5", edgeType: "et3", tags: "created"),
        new GraphEdge("node4", "node3", edgeType : "et3", tags: "created"),
        new GraphEdge("node5", "node4", edgeType : "et2", tags: "created"),
    };
    private readonly ITestOutputHelper _outputHelper;

    public SelectEdgeInstructionTests(ITestOutputHelper outputHelper)
    {
        _outputHelper = outputHelper;
    }

    [Fact]
    public async Task SelectAllEdges()
    {
        using GraphHostService testClient = await TestApplication.CreateTestGraphService(_map.Clone(), _outputHelper);
        var collector = testClient.Services.GetRequiredService<GraphMapCounter>();
        var context = testClient.CreateScopeContext<SelectEdgeInstructionTests>();

        var newMapOption = await testClient.Execute("select [*] ;", context);
        newMapOption.IsOk().Should().BeTrue();

        QueryResult result = newMapOption.Return();
        result.Option.IsOk().Should().BeTrue();
        result.Alias.Should().NotBeEmpty();
        result.Nodes.Count.Should().Be(0);
        result.Edges.Count.Should().Be(6);
        result.DataLinks.Count.Should().Be(0);

        var expected = new (string FromKey, string ToKey, string EdgeType)[]
        {
            ("node1", "node2", "et1"),
            ("node1", "node3", "et1"),
            ("node6", "node3", "et3"),
            ("node4", "node5", "et3"),
            ("node4", "node3", "et3"),
            ("node5", "node4", "et2"),
        }.OrderBy(x => x).ToArray();

        result.Edges.Select(x => (x.FromKey, x.ToKey, x.EdgeType)).OrderBy(x => x).SequenceEqual(expected).Should().BeTrue();

        collector.Edges.IndexHit.Value.Should().Be(0);
        collector.Edges.IndexMissed.Value.Should().Be(0);
        collector.Edges.IndexScan.Value.Should().Be(1);
    }

    [Fact]
    public async Task SelectAllEdgesWithTag()
    {
        using GraphHostService testClient = await TestApplication.CreateTestGraphService(_map.Clone(), _outputHelper);
        var collector = testClient.Services.GetRequiredService<GraphMapCounter>();
        var context = testClient.CreateScopeContext<SelectEdgeInstructionTests>();

        var newMapOption = await testClient.Execute("select [*, level=1] ;", context);
        newMapOption.IsOk().Should().BeTrue();

        QueryResult result = newMapOption.Return();
        result.Option.IsOk().Should().BeTrue();
        result.Alias.Should().NotBeEmpty();
        result.Nodes.Count.Should().Be(0);
        result.Edges.Count.Should().Be(2);
        result.DataLinks.Count.Should().Be(0);

        var expected = new (string FromKey, string ToKey, string EdgeType)[]
        {
            ("node1", "node2", "et1"),
            ("node1", "node3", "et1"),
        }.OrderBy(x => x).ToArray();

        result.Edges.Select(x => (x.FromKey, x.ToKey, x.EdgeType)).OrderBy(x => x).SequenceEqual(expected).Should().BeTrue();

        collector.Edges.IndexHit.Value.Should().Be(0);
        collector.Edges.IndexMissed.Value.Should().Be(0);
        collector.Edges.IndexScan.Value.Should().Be(1);
    }

    [Fact]
    public async Task SelectEdgesWithTag()
    {
        using GraphHostService testClient = await TestApplication.CreateTestGraphService(_map.Clone(), _outputHelper);
        var collector = testClient.Services.GetRequiredService<GraphMapCounter>();
        var context = testClient.CreateScopeContext<SelectEdgeInstructionTests>();

        var newMapOption = await testClient.Execute("select [level=1] ;", context);
        newMapOption.IsOk().Should().BeTrue();

        QueryResult result = newMapOption.Return();
        result.Option.IsOk().Should().BeTrue();
        result.Alias.Should().NotBeEmpty();
        result.Nodes.Count.Should().Be(0);
        result.Edges.Count.Should().Be(2);
        result.DataLinks.Count.Should().Be(0);

        var expected = new (string FromKey, string ToKey, string EdgeType)[]
        {
            ("node1", "node2", "et1"),
            ("node1", "node3", "et1"),
        }.OrderBy(x => x).ToArray();

        result.Edges.Select(x => (x.FromKey, x.ToKey, x.EdgeType)).OrderBy(x => x).SequenceEqual(expected).Should().BeTrue();

        collector.Edges.IndexHit.Value.Should().Be(3);
        collector.Edges.IndexMissed.Value.Should().Be(0);
        collector.Edges.IndexScan.Value.Should().Be(0);
    }

    [Fact]
    public async Task SelectEdgesByFrom()
    {
        using GraphHostService testClient = await TestApplication.CreateTestGraphService(_map.Clone(), _outputHelper);
        var collector = testClient.Services.GetRequiredService<GraphMapCounter>();
        var context = testClient.CreateScopeContext<SelectEdgeInstructionTests>();

        var newMapOption = await testClient.Execute("select [from=node1] ;", context);
        newMapOption.IsOk().Should().BeTrue();

        QueryResult result = newMapOption.Return();
        result.Option.IsOk().Should().BeTrue();
        result.Alias.Should().NotBeEmpty();
        result.Nodes.Count.Should().Be(0);
        result.Edges.Count.Should().Be(2);
        result.DataLinks.Count.Should().Be(0);

        var expected = new (string FromKey, string ToKey, string EdgeType)[]
        {
            ("node1", "node2", "et1"),
            ("node1", "node3", "et1"),
        }.OrderBy(x => x).ToArray();

        result.Edges.Select(x => (x.FromKey, x.ToKey, x.EdgeType)).OrderBy(x => x).SequenceEqual(expected).Should().BeTrue();

        collector.Edges.IndexHit.Value.Should().Be(3);
        collector.Edges.IndexMissed.Value.Should().Be(0);
        collector.Edges.IndexScan.Value.Should().Be(0);
    }

    [Fact]
    public async Task SelectEdgesByTo()
    {
        using GraphHostService testClient = await TestApplication.CreateTestGraphService(_map.Clone(), _outputHelper);
        var collector = testClient.Services.GetRequiredService<GraphMapCounter>();
        var context = testClient.CreateScopeContext<SelectEdgeInstructionTests>();

        var newMapOption = await testClient.Execute("select [to=node3] ;", context);
        newMapOption.IsOk().Should().BeTrue();

        QueryResult result = newMapOption.Return();
        result.Option.IsOk().Should().BeTrue();
        result.Alias.Should().NotBeEmpty();
        result.Nodes.Count.Should().Be(0);
        result.Edges.Count.Should().Be(3);
        result.DataLinks.Count.Should().Be(0);

        var expected = new (string FromKey, string ToKey, string EdgeType)[]
        {
            ("node1", "node3", "et1"),
            ("node6", "node3", "et3"),
            ("node4", "node3", "et3"),
        }.OrderBy(x => x).ToArray();

        result.Edges.Select(x => (x.FromKey, x.ToKey, x.EdgeType)).OrderBy(x => x).SequenceEqual(expected).Should().BeTrue();

        collector.Edges.IndexHit.Value.Should().Be(4);
        collector.Edges.IndexMissed.Value.Should().Be(0);
        collector.Edges.IndexScan.Value.Should().Be(0);
    }

    [Fact]
    public async Task SelectEdgesByEdgeType()
    {
        using GraphHostService testClient = await TestApplication.CreateTestGraphService(_map.Clone(), _outputHelper);
        var collector = testClient.Services.GetRequiredService<GraphMapCounter>();
        var context = testClient.CreateScopeContext<SelectEdgeInstructionTests>();

        var newMapOption = await testClient.Execute("select [type=et3] ;", context);
        newMapOption.IsOk().Should().BeTrue();

        QueryResult result = newMapOption.Return();
        result.Option.IsOk().Should().BeTrue();
        result.Alias.Should().NotBeEmpty();
        result.Nodes.Count.Should().Be(0);
        result.Edges.Count.Should().Be(3);
        result.DataLinks.Count.Should().Be(0);

        var expected = new (string FromKey, string ToKey, string EdgeType)[]
        {
            ("node6", "node3", "et3"),
            ("node4", "node5", "et3"),
            ("node4", "node3", "et3"),
        }.OrderBy(x => x).ToArray();

        result.Edges.Select(x => (x.FromKey, x.ToKey, x.EdgeType)).OrderBy(x => x).SequenceEqual(expected).Should().BeTrue();

        collector.Edges.IndexHit.Value.Should().Be(4);
        collector.Edges.IndexMissed.Value.Should().Be(0);
        collector.Edges.IndexScan.Value.Should().Be(0);
    }
}
