﻿using FluentAssertions;
using Toolbox.Types;

namespace Toolbox.Graph.test.Graph.Map;

public class NodeIndexTests
{
    private readonly GraphMap _map = new GraphMap()
    {
        new GraphNode("node1", tags: "name=marko,age=29", indexes: "name"),
        new GraphNode("node2", tags: "name=vadas,age=27"),
        new GraphNode("node3", tags: "name=lop,lang=java"),
        new GraphNode("node4", tags: "name=josh,age=32"),
        new GraphNode("node5", tags: "name=ripple,lang=java", indexes: "lang, name"),
        new GraphNode("node6", tags: "name=peter,age=35"),
        new GraphNode("node7", tags: "lang=java"),

        new GraphEdge("node1", "node2", edgeType: "et1", tags: "knows,level=1"),
        new GraphEdge("node1", "node3", edgeType: "et1", tags: "knows,level=1"),
        new GraphEdge("node6", "node3", edgeType: "et1", tags: "created"),
        new GraphEdge("node4", "node5", edgeType: "et1", tags: "created"),
        new GraphEdge("node4", "node3", edgeType : "et1", tags: "created"),
    };

    [Fact]
    public void VerifyTagIndex1()
    {
        var subject = _map.Nodes.LookupTag("name");
        subject.Count.Should().Be(6);
        subject.Should().BeEquivalentTo(["node1", "node2", "node3", "node4", "node5", "node6"]);

        var subjects = _map.Nodes.LookupTaggedNodes("name");
        subjects.Count.Should().Be(6);
        subjects.Select(x => x.Key).Should().BeEquivalentTo(["node1", "node2", "node3", "node4", "node5", "node6"]);
    }

    [Fact]
    public void VerifyTagIndex2()
    {
        var subject = _map.Nodes.LookupTag("lang");
        subject.Count.Should().Be(3);
        subject.Should().BeEquivalentTo(["node3", "node5", "node7"]);

        var subjects = _map.Nodes.LookupTaggedNodes("lang");
        subjects.Count.Should().Be(3);
        subjects.Select(x => x.Key).Should().BeEquivalentTo(["node3", "node5", "node7"]);
    }

    [Fact]
    public void VerifyEdgeIndex1()
    {
        var subject = _map.Edges.LookupTag("knows");
        subject.Count.Should().Be(2);
        subject.Should().BeEquivalentTo([
            new GraphEdgePrimaryKey { FromKey = "node1", ToKey = "node2", EdgeType = "et1" },
            new GraphEdgePrimaryKey { FromKey = "node1", ToKey = "node3", EdgeType = "et1" },
            ]
        );
    }

    [Fact]
    public void VerifyEdgeIndex2()
    {
        var subject = _map.Edges.LookupTag("created");
        subject.Count.Should().Be(3);
        subject.Should().BeEquivalentTo([
            new GraphEdgePrimaryKey { FromKey = "node6", ToKey = "node3", EdgeType = "et1" },
            new GraphEdgePrimaryKey { FromKey = "node4", ToKey = "node5", EdgeType = "et1" },
            new GraphEdgePrimaryKey { FromKey = "node4", ToKey = "node3", EdgeType = "et1" },
            ]
        );
    }

    [Fact]
    public void LookupNodeIndexValue()
    {
        var subject = _map.Nodes.LookupIndex("name", "marko");
        subject.IsOk().Should().BeTrue();
        subject.Return().NodeKey.Should().Be("node1");
    }

    [Fact]
    public void LookupNodeIndexValue2()
    {
        var subject = _map.Nodes.LookupIndex("lang", "java");
        subject.IsOk().Should().BeTrue();
        subject.Return().NodeKey.Should().Be("node5");
    }

    [Fact]
    public void FailToLookupIndexValue()
    {
        var subject = _map.Nodes.LookupIndex("name", "marko2");
        subject.IsError().Should().BeTrue();
    }
}
