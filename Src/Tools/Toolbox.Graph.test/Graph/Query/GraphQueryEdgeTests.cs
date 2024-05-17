﻿using FluentAssertions;
using Toolbox.Extensions;
using Toolbox.Types;

namespace Toolbox.Graph.test.Graph.Query;

public class GraphQueryEdgeTests
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
    public async Task AllEdgesQuery()
    {
        var testClient = GraphTestStartup.CreateGraphTestHost(_map);
        GraphQueryResult result = (await testClient.Execute("select [*];", NullScopeContext.Instance)).ThrowOnError().Return();

        result.Status.IsOk().Should().BeTrue();
        result.Items.Length.Should().Be(5);
        result.Alias.Count.Should().Be(0);

        var edges = result.Items.Select(x => x.Cast<GraphEdge>()).ToArray();
        edges.Length.Should().Be(5);

        var shouldMatch = new[]
        {
            ("node1", "node2"),
            ("node1", "node3"),
            ("node6", "node3"),
            ("node4", "node3"),
            ("node4", "node5"),
        };
        var inSet = edges.OrderBy(x => x.ToKey).Select(x => (x.FromKey, x.ToKey)).ToArray();
        inSet.SequenceEqual(shouldMatch).Should().BeTrue();
    }

    [Fact]
    public async Task TagDefaultQuery()
    {
        var testClient = GraphTestStartup.CreateGraphTestHost(_map);
        GraphQueryResult result = (await testClient.Execute("select [knows];", NullScopeContext.Instance)).ThrowOnError().Return();

        result.Status.IsOk().Should().BeTrue();
        result.Items.Length.Should().Be(2);
        result.Alias.Count.Should().Be(0);

        var edges = result.Items.Select(x => x.Cast<GraphEdge>()).ToArray();
        edges.Length.Should().Be(2);

        var shouldMatch = new[] { ("node1", "node2"), ("node1", "node3") };
        var inSet = edges.OrderBy(x => x.ToKey).Select(x => (x.FromKey, x.ToKey)).ToArray();
        inSet.SequenceEqual(shouldMatch).Should().BeTrue();
    }

    [Fact]
    public async Task TagWithKeywordQuery()
    {
        var testClient = GraphTestStartup.CreateGraphTestHost(_map);
        GraphQueryResult result = (await testClient.Execute("select [knows];", NullScopeContext.Instance)).ThrowOnError().Return();

        result.Status.IsOk().Should().BeTrue();
        result.Items.Length.Should().Be(2);
        result.Alias.Count.Should().Be(0);

        var edges = result.Items.Select(x => x.Cast<GraphEdge>()).ToArray();
        edges.Length.Should().Be(2);

        var shouldMatch = new[] { ("node1", "node2"), ("node1", "node3") };
        var inSet = edges.OrderBy(x => x.ToKey).Select(x => (x.FromKey, x.ToKey)).ToArray();
        inSet.SequenceEqual(shouldMatch).Should().BeTrue();
    }

    [Fact]
    public async Task TagWithKeywordForSpecificQuery()
    {
        var testClient = GraphTestStartup.CreateGraphTestHost(_map);
        GraphQueryResult result = (await testClient.Execute("select [level=1];", NullScopeContext.Instance)).ThrowOnError().Return();

        result.Status.IsOk().Should().BeTrue();
        result.Items.Length.Should().Be(2);
        result.Alias.Count.Should().Be(0);

        var edges = result.Items.Select(x => x.Cast<GraphEdge>()).ToArray();
        edges.Length.Should().Be(2);

        var inSet = _map.Edges.Where(x => x.Tags.Has("level=1")).Select(x => x.Key).OrderBy(x => x).ToArray();
        edges.Select(x => x.Key).OrderBy(y => y).SequenceEqual(inSet).Should().BeTrue();
    }

    [Fact]
    public async Task TagWithFromAndToQuery()
    {
        var testClient = GraphTestStartup.CreateGraphTestHost(_map);
        GraphQueryResult result = (await testClient.Execute("select [fromKey=node1, toKey=node2];", NullScopeContext.Instance)).ThrowOnError().Return();

        result.Status.IsOk().Should().BeTrue();
        result.Items.Length.Should().Be(1);
        result.Alias.Count.Should().Be(0);

        var edges = result.Items.Select(x => x.Cast<GraphEdge>()).ToArray();
        edges.Length.Should().Be(1);

        var inSet = _map.Edges.Where(x => x.FromKey == "node1" && x.ToKey == "node2").Select(x => x.Key).ToArray();
        edges.Select(x => x.Key).OrderBy(y => y).SequenceEqual(inSet).Should().BeTrue();
    }
}
