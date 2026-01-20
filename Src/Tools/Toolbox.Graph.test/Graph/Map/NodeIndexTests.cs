using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Toolbox.Tools;
using Toolbox.Types;
using Xunit.Abstractions;

namespace Toolbox.Graph.test.Graph.Map;

public class NodeIndexTests
{
    private readonly ILogger<GraphMap> _logger;
    private readonly IServiceProvider _services;
    private readonly GraphMap _map;

    public NodeIndexTests(ITestOutputHelper output)
    {
        var host = Host.CreateDefaultBuilder()
            .AddDebugLogging(x => output.WriteLine(x))
            .Build();

        _logger = host.Services.GetRequiredService<ILogger<GraphMap>>();
        _services = host.Services;

        _map = new GraphMap(_logger)
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
    }

    [Fact]
    public void VerifyTagIndex1()
    {
        var subject = _map.Nodes.LookupTag("name");
        subject.Count.Be(6);
        subject.OrderBy(x => x).SequenceEqual(["node1", "node2", "node3", "node4", "node5", "node6"]).BeTrue();

        var subjects = _map.Nodes.LookupTaggedNodes("name");
        subjects.Count.Be(6);
        subjects.Select(x => x.Key).OrderBy(x => x).SequenceEqual(["node1", "node2", "node3", "node4", "node5", "node6"]).BeTrue();
    }

    [Fact]
    public void VerifyTagIndex2()
    {
        var subject = _map.Nodes.LookupTag("lang");
        subject.Count.Be(3);
        subject.OrderBy(x => x).SequenceEqual(["node3", "node5", "node7"]).BeTrue();

        var subjects = _map.Nodes.LookupTaggedNodes("lang");
        subjects.Count.Be(3);
        subjects.Select(x => x.Key).OrderBy(x => x).SequenceEqual(["node3", "node5", "node7"]).BeTrue();
    }

    [Fact]
    public void VerifyEdgeIndex1()
    {
        var subject = _map.Edges.LookupTag("knows").OrderBy(x => x.ToString()).ToList();
        subject.Count.Be(2);
        var compareTo = new List<GraphEdgePrimaryKey>
        {
            new GraphEdgePrimaryKey { FromKey = "node1", ToKey = "node2", EdgeType = "et1" },
            new GraphEdgePrimaryKey { FromKey = "node1", ToKey = "node3", EdgeType = "et1" },
        };
        subject.SequenceEqual(compareTo).BeTrue();
    }

    [Fact]
    public void VerifyEdgeIndex2()
    {
        var subject = _map.Edges.LookupTag("created").ToHashSet();
        subject.Count.Be(3);
        var compareTo = new HashSet<GraphEdgePrimaryKey>
        {
            new GraphEdgePrimaryKey { FromKey = "node6", ToKey = "node3", EdgeType = "et1" },
            new GraphEdgePrimaryKey { FromKey = "node4", ToKey = "node5", EdgeType = "et1" },
            new GraphEdgePrimaryKey { FromKey = "node4", ToKey = "node3", EdgeType = "et1" },
        };
        subject.SetEquals(compareTo).BeTrue();
    }

    [Fact]
    public void LookupNodeIndexValue()
    {
        var subject = _map.Nodes.LookupIndex("name", "marko");
        subject.IsOk().BeTrue();
        subject.Return().NodeKey.Be("node1");
    }

    [Fact]
    public void LookupNodeIndexValue2()
    {
        var subject = _map.Nodes.LookupIndex("lang", "java");
        subject.IsOk().BeTrue();
        subject.Return().NodeKey.Be("node5");
    }

    [Fact]
    public void FailToLookupIndexValue()
    {
        var subject = _map.Nodes.LookupIndex("name", "marko2");
        subject.IsError().BeTrue();
    }
}
