using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;
using Xunit.Abstractions;

namespace Toolbox.Graph.test.Graph.Map;

public class NodeUniqueIndexTests
{
    private readonly ILogger<GraphMap> _logger;

    public NodeUniqueIndexTests(ITestOutputHelper output)
    {
        var host = Host.CreateDefaultBuilder()
            .AddDebugLogging(x => output.WriteLine(x))
            .Build();

        _logger = host.Services.GetRequiredService<ILogger<GraphMap>>();
    }

    [Fact]
    public void SingleNodeIndexed()
    {
        var map = new GraphMap(_logger)
        {
            new GraphNode("node1", tags: "name=marko,age=29", indexes: "name"),
        };

        var subject = map.Nodes.LookupIndex("name", "marko");
        subject.IsOk().BeTrue();
        subject.Return().NodeKey.Be("node1");

        map.Nodes.LookupIndex("name", "marko").Action(x =>
        {
            x.IsOk().BeTrue();
            var subject = x.Return();
            subject.NodeKey.Be("node1");
            subject.IndexName.Be("name");
            subject.Value.Be("marko");
        });

        map.Nodes.LookupByNodeKey("node8").Count.Be(0);

        map.Nodes.LookupByNodeKey("node1").Action(nodes =>
        {
            nodes.Count.Be(1);
            nodes[0].NodeKey.Be("node1");
            nodes[0].IndexName.Be("name");
            nodes[0].Value.Be("marko");
        });

        var duplicateNode = new GraphNode("node1");
        map.Nodes.Add(duplicateNode).IsError().BeTrue();

        map.Nodes.Set(duplicateNode).IsOk().BeTrue();

        map.Nodes.TryGetValue("node1", out var readNode).BeTrue();
        readNode.NotNull();
        readNode!.Key.Be("node1");
        readNode.TagsString.Be("age=29,name=marko");

    }

    [Fact]
    public void RemoveSingleNodeIndexed()
    {
        var map = new GraphMap(_logger)
        {
            new GraphNode("node1", tags: "name=marko,age=29", indexes: "name"),
        };

        var subject = map.Nodes.LookupIndex("name", "marko");
        subject.IsOk().BeTrue();
        subject.Return().NodeKey.Be("node1");

        map.Nodes.LookupByNodeKey("node1").Count.Be(1);
        map.Nodes.LookupIndex("name", "marko").IsOk().BeTrue();

        var updatedNode = new GraphNode("node1", indexes: "-name");
        var setResult = map.Nodes.Set(updatedNode);
        setResult.IsOk().BeTrue();

        map.Nodes.LookupByNodeKey("node1").Count.Be(0);
        map.Nodes.LookupIndex("name", "marko").IsNotFound().BeTrue();

        map.Nodes.TryGetValue("node1", out var readNode).BeTrue();
        readNode!.Key.Be("node1");
        readNode.TagsString.Be("age=29,name=marko");
        readNode.IndexesString.BeEmpty();
    }

    [Fact]
    public void SingleNodeTwoTagsIndexed()
    {
        var map = new GraphMap(_logger)
        {
            new GraphNode("node5", tags: "name=ripple,lang=java", indexes: "lang, name"),
        };

        map.Nodes.LookupIndex("name", "ripple").Action(x =>
        {
            x.IsOk().BeTrue();
            x.Return().NodeKey.Be("node5");
        });

        map.Nodes.LookupIndex("lang", "java").Action(x =>
        {
            x.IsOk().BeTrue();
            x.Return().NodeKey.Be("node5");
        });

        map.Nodes.LookupIndex("lang", "java2").IsError().BeTrue();
        map.Nodes.LookupIndex("lang2", "java").IsError().BeTrue();

        map.Nodes.LookupByNodeKey("node8").Count.Be(0);

        map.Nodes.LookupByNodeKey("node5").Action(nodes =>
        {
            nodes.Count.Be(2);
            nodes.Count.Be(2);
            nodes.Count(x => x.IndexName == "name" && x.Value == "ripple").Be(1);
            nodes.Count(x => x.IndexName == "lang" && x.Value == "java").Be(1);
        });
    }

    [Fact]
    public void TwoNodeIndexed()
    {
        var map = new GraphMap(_logger)
        {
            new GraphNode("node2", tags: "name=vadas,age=27"),
            new GraphNode("node1", tags: "name=marko,age=29", indexes: "name"),
            new GraphNode("node5", tags: "name=ripple,lang=java", indexes: "lang, name"),
        };

        map.Nodes.LookupIndex("name", "marko").Action(x =>
        {
            x.IsOk().BeTrue();
            x.Return().NodeKey.Be("node1");
        });

        map.Nodes.LookupIndex("name", "ripple").Action(x =>
        {
            x.IsOk().BeTrue();
            x.Return().NodeKey.Be("node5");
        });

        map.Nodes.LookupIndex("lang", "java").Action(x =>
        {
            x.IsOk().BeTrue();
            x.Return().NodeKey.Be("node5");
        });

        map.Nodes.LookupIndex("lang", "java2").IsError().BeTrue();
        map.Nodes.LookupIndex("lang2", "java").IsError().BeTrue();

        map.Nodes.LookupByNodeKey("node1").Action(nodes =>
        {
            nodes.Count.Be(1);
            nodes[0].NodeKey.Be("node1");
            nodes[0].IndexName.Be("name");
            nodes[0].Value.Be("marko");
        });

        map.Nodes.LookupByNodeKey("node5").Action(nodes =>
        {
            nodes.Count.Be(2);
            nodes.Count(x => x.IndexName == "name" && x.Value == "ripple").Be(1);
            nodes.Count(x => x.IndexName == "lang" && x.Value == "java").Be(1);
        });
    }

    [Fact]
    public void UpdateIndexOnOneNode()
    {
        var map = new GraphMap(_logger)
        {
            new GraphNode("node2", tags: "name=vadas,age=27"),
            new GraphNode("node1", tags: "name=marko,age=29", indexes: "name"),
        };

        map.Nodes.LookupIndex("name", "marko").Action(subject =>
        {
            subject.IsOk().BeTrue();
            subject.Return().NodeKey.Be("node1");
        });

        var updatedNode = new GraphNode("node1", tags: "name=marko2,age=29", indexes: "name");
        var setResult = map.Nodes.Set(updatedNode);
        setResult.IsOk().BeTrue();

        map.Nodes.LookupIndex("name", "marko").IsError().BeTrue();

        map.Nodes.LookupIndex("name", "marko2").Action(subject =>
        {
            subject.IsOk().BeTrue();
            subject.Return().NodeKey.Be("node1");
        });

        map.Nodes.LookupByNodeKey("node1").Action(nodes =>
        {
            nodes.Count.Be(1);
            nodes[0].NodeKey.Be("node1");
            nodes[0].IndexName.Be("name");
            nodes[0].Value.Be("marko2");
        });
    }

    [Fact]
    public void UpdateIndexOnOneNodeIndexCarryForward()
    {
        var map = new GraphMap(_logger)
        {
            new GraphNode("node2", tags: "name=vadas,age=27"),
            new GraphNode("node1", tags: "name=marko,age=29", indexes: "name"),
        };

        map.Nodes.LookupIndex("name", "marko").Action(subject =>
        {
            subject.IsOk().BeTrue();
            subject.Return().NodeKey.Be("node1");
        });

        var updatedNode = new GraphNode("node1", tags: "name=marko2,age=29");
        var setResult = map.Nodes.Set(updatedNode);
        setResult.IsOk().BeTrue();

        map.Nodes.TryGetValue("node1", out var readNode).Action(x =>
        {
            x.BeTrue();
            readNode!.Key.Be("node1");
            readNode.TagsString.Be("age=29,name=marko2");
            readNode.IndexesString.Be("name");
        });

        map.Nodes.LookupIndex("name", "marko").IsError().BeTrue();

        map.Nodes.LookupIndex("name", "marko2").Action(subject =>
        {
            subject.IsOk().BeTrue();
            subject.Return().NodeKey.Be("node1");
        });

        map.Nodes.LookupByNodeKey("node1").Action(nodes =>
        {
            nodes.Count.Be(1);
            nodes[0].NodeKey.Be("node1");
            nodes[0].IndexName.Be("name");
            nodes[0].Value.Be("marko2");
        });
    }


    [Fact]
    public void RemoveIndexFromMultipleIndexes()
    {
        var map = new GraphMap(_logger)
        {
            new GraphNode("node2", tags: "name=vadas,age=27"),
            new GraphNode("node1", tags: "name=marko,age=29", indexes: "name"),
            new GraphNode("node5", tags: "name=ripple,lang=java", indexes: "lang, name"),
        };

        map.Nodes.LookupIndex("name", "marko").Action(x =>
        {
            x.IsOk().BeTrue();
            x.Return().NodeKey.Be("node1");
        });

        map.Nodes.LookupIndex("name", "ripple").Action(x =>
        {
            x.IsOk().BeTrue();
            x.Return().NodeKey.Be("node5");
        });

        map.Nodes.LookupIndex("lang", "java").Action(x =>
        {
            x.IsOk().BeTrue();
            x.Return().NodeKey.Be("node5");
        });

        map.Nodes.LookupByNodeKey("node1").Action(nodes =>
        {
            nodes.Count.Be(1);
            nodes[0].NodeKey.Be("node1");
            nodes[0].IndexName.Be("name");
            nodes[0].Value.Be("marko");
        });

        map.Nodes.LookupByNodeKey("node5").Action(nodes =>
        {
            nodes.Count.Be(2);
            nodes.Count(x => x.IndexName == "name" && x.Value == "ripple").Be(1);
            nodes.Count(x => x.IndexName == "lang" && x.Value == "java").Be(1);
        });

        var updatedNode = new GraphNode("node5", tags: "-lang");
        var setResult = map.Nodes.Set(updatedNode);
        setResult.IsOk().BeTrue();

        map.Nodes.LookupIndex("lang", "java").IsNotFound().BeTrue();

        map.Nodes.LookupIndex("name", "marko").Action(x =>
        {
            x.IsOk().BeTrue();
            x.Return().NodeKey.Be("node1");
        });

        map.Nodes.LookupIndex("name", "ripple").Action(x =>
        {
            x.IsOk().BeTrue();
            x.Return().NodeKey.Be("node5");
        });

        map.Nodes.LookupByNodeKey("node5").Action(nodes =>
        {
            nodes.Count.Be(1);
            nodes.Count(x => x.IndexName == "name" && x.Value == "ripple").Be(1);
            nodes.Count(x => x.IndexName == "lang" && x.Value == "java").Be(0);
        });

        map.Nodes.TryGetValue("node5", out var readNode).BeTrue();
        readNode.NotNull();
        readNode!.Key.Be("node5");
        readNode.TagsString.Be("name=ripple");
    }

    [Fact]
    public void RemoveNameIndexFromMultipleIndexes()
    {
        var map = new GraphMap(_logger)
        {
            new GraphNode("node2", tags: "name=vadas,age=27"),
            new GraphNode("node1", tags: "name=marko,age=29", indexes: "name"),
            new GraphNode("node5", tags: "name=ripple,lang=java", indexes: "lang, name"),
        };

        map.Nodes.LookupIndex("name", "marko").Action(x =>
        {
            x.IsOk().BeTrue();
            x.Return().NodeKey.Be("node1");
        });

        map.Nodes.LookupIndex("name", "ripple").Action(x =>
        {
            x.IsOk().BeTrue();
            x.Return().NodeKey.Be("node5");
        });

        map.Nodes.LookupIndex("lang", "java").Action(x =>
        {
            x.IsOk().BeTrue();
            x.Return().NodeKey.Be("node5");
        });

        map.Nodes.LookupByNodeKey("node1").Action(nodes =>
        {
            nodes.Count.Be(1);
            nodes[0].NodeKey.Be("node1");
            nodes[0].IndexName.Be("name");
            nodes[0].Value.Be("marko");
        });

        map.Nodes.LookupByNodeKey("node5").Action(nodes =>
        {
            nodes.Count.Be(2);
            nodes.Count(x => x.IndexName == "name" && x.Value == "ripple").Be(1);
            nodes.Count(x => x.IndexName == "lang" && x.Value == "java").Be(1);
        });

        var updatedNode = new GraphNode("node5", tags: "-name, -lang");
        var setResult = map.Nodes.Set(updatedNode);
        setResult.IsOk().BeTrue();

        map.Nodes.LookupIndex("lang", "java").IsNotFound().BeTrue();
        map.Nodes.LookupIndex("name", "ripple").IsNotFound().BeTrue();

        map.Nodes.LookupByNodeKey("node5").Action(nodes =>
        {
            nodes.Count.Be(0);
            nodes.Count(x => x.IndexName == "name" && x.Value == "ripple").Be(0);
            nodes.Count(x => x.IndexName == "lang" && x.Value == "java").Be(0);
        });

        map.Nodes.TryGetValue("node5", out var readNode).BeTrue();
        readNode.NotNull();
        readNode!.Key.Be("node5");
        readNode.Tags.Count.Be(0);
    }
}
