using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Tools.Should;
using Toolbox.Types;

namespace Toolbox.Graph.test.Graph.Map;

public class NodeUniqueIndexTests
{
    [Fact]
    public void SingleNodeIndexed()
    {
        var map = new GraphMap()
        {
            new GraphNode("node1", tags: "name=marko,age=29", indexes: "name"),
        };

        var subject = map.Nodes.LookupIndex("name", "marko");
        subject.IsOk().Should().BeTrue();
        subject.Return().NodeKey.Should().Be("node1");

        map.Nodes.LookupIndex("name", "marko").Action(x =>
        {
            x.IsOk().Should().BeTrue();
            var subject = x.Return();
            subject.NodeKey.Should().Be("node1");
            subject.IndexName.Should().Be("name");
            subject.Value.Should().Be("marko");
        });

        map.Nodes.LookupByNodeKey("node8").Count.Should().Be(0);

        map.Nodes.LookupByNodeKey("node1").Action(nodes =>
        {
            nodes.Count.Should().Be(1);
            nodes[0].NodeKey.Should().Be("node1");
            nodes[0].IndexName.Should().Be("name");
            nodes[0].Value.Should().Be("marko");
        });

        var duplicateNode = new GraphNode("node1");
        map.Nodes.Add(duplicateNode).IsError().Should().BeTrue();

        map.Nodes.Set(duplicateNode).IsOk().Should().BeTrue();

        map.Nodes.TryGetValue("node1", out var readNode).Should().BeTrue();
        readNode.NotNull();
        readNode!.Key.Should().Be("node1");
        readNode.TagsString.Should().Be("age=29,name=marko");

    }

    [Fact]
    public void RemoveSingleNodeIndexed()
    {
        var map = new GraphMap()
        {
            new GraphNode("node1", tags: "name=marko,age=29", indexes: "name"),
        };

        var subject = map.Nodes.LookupIndex("name", "marko");
        subject.IsOk().Should().BeTrue();
        subject.Return().NodeKey.Should().Be("node1");

        map.Nodes.LookupByNodeKey("node1").Count.Should().Be(1);
        map.Nodes.LookupIndex("name", "marko").IsOk().Should().BeTrue();

        var updatedNode = new GraphNode("node1", indexes: "-name");
        var setResult = map.Nodes.Set(updatedNode);
        setResult.IsOk().Should().BeTrue();

        map.Nodes.LookupByNodeKey("node1").Count.Should().Be(0);
        map.Nodes.LookupIndex("name", "marko").IsNotFound().Should().BeTrue();

        map.Nodes.TryGetValue("node1", out var readNode).Should().BeTrue();
        readNode!.Key.Should().Be("node1");
        readNode.TagsString.Should().Be("age=29,name=marko");
        readNode.IndexesString.Should().BeEmpty();
    }

    [Fact]
    public void SingleNodeTwoTagsIndexed()
    {
        var map = new GraphMap()
        {
            new GraphNode("node5", tags: "name=ripple,lang=java", indexes: "lang, name"),
        };

        map.Nodes.LookupIndex("name", "ripple").Action(x =>
        {
            x.IsOk().Should().BeTrue();
            x.Return().NodeKey.Should().Be("node5");
        });

        map.Nodes.LookupIndex("lang", "java").Action(x =>
        {
            x.IsOk().Should().BeTrue();
            x.Return().NodeKey.Should().Be("node5");
        });

        map.Nodes.LookupIndex("lang", "java2").IsError().Should().BeTrue();
        map.Nodes.LookupIndex("lang2", "java").IsError().Should().BeTrue();

        map.Nodes.LookupByNodeKey("node8").Count.Should().Be(0);

        map.Nodes.LookupByNodeKey("node5").Action(nodes =>
        {
            nodes.Count.Should().Be(2);
            nodes.Count.Should().Be(2);
            nodes.Count(x => x.IndexName == "name" && x.Value == "ripple").Should().Be(1);
            nodes.Count(x => x.IndexName == "lang" && x.Value == "java").Should().Be(1);
        });
    }

    [Fact]
    public void TwoNodeIndexed()
    {
        var map = new GraphMap()
        {
            new GraphNode("node2", tags: "name=vadas,age=27"),
            new GraphNode("node1", tags: "name=marko,age=29", indexes: "name"),
            new GraphNode("node5", tags: "name=ripple,lang=java", indexes: "lang, name"),
        };

        map.Nodes.LookupIndex("name", "marko").Action(x =>
        {
            x.IsOk().Should().BeTrue();
            x.Return().NodeKey.Should().Be("node1");
        });

        map.Nodes.LookupIndex("name", "ripple").Action(x =>
        {
            x.IsOk().Should().BeTrue();
            x.Return().NodeKey.Should().Be("node5");
        });

        map.Nodes.LookupIndex("lang", "java").Action(x =>
        {
            x.IsOk().Should().BeTrue();
            x.Return().NodeKey.Should().Be("node5");
        });

        map.Nodes.LookupIndex("lang", "java2").IsError().Should().BeTrue();
        map.Nodes.LookupIndex("lang2", "java").IsError().Should().BeTrue();

        map.Nodes.LookupByNodeKey("node1").Action(nodes =>
        {
            nodes.Count.Should().Be(1);
            nodes[0].NodeKey.Should().Be("node1");
            nodes[0].IndexName.Should().Be("name");
            nodes[0].Value.Should().Be("marko");
        });

        map.Nodes.LookupByNodeKey("node5").Action(nodes =>
        {
            nodes.Count.Should().Be(2);
            nodes.Count(x => x.IndexName == "name" && x.Value == "ripple").Should().Be(1);
            nodes.Count(x => x.IndexName == "lang" && x.Value == "java").Should().Be(1);
        });
    }

    [Fact]
    public void UpdateIndexOnOneNode()
    {
        var map = new GraphMap()
        {
            new GraphNode("node2", tags: "name=vadas,age=27"),
            new GraphNode("node1", tags: "name=marko,age=29", indexes: "name"),
        };

        map.Nodes.LookupIndex("name", "marko").Action(subject =>
        {
            subject.IsOk().Should().BeTrue();
            subject.Return().NodeKey.Should().Be("node1");
        });

        var updatedNode = new GraphNode("node1", tags: "name=marko2,age=29", indexes: "name");
        var setResult = map.Nodes.Set(updatedNode);
        setResult.IsOk().Should().BeTrue();

        map.Nodes.LookupIndex("name", "marko").IsError().Should().BeTrue();

        map.Nodes.LookupIndex("name", "marko2").Action(subject =>
        {
            subject.IsOk().Should().BeTrue();
            subject.Return().NodeKey.Should().Be("node1");
        });

        map.Nodes.LookupByNodeKey("node1").Action(nodes =>
        {
            nodes.Count.Should().Be(1);
            nodes[0].NodeKey.Should().Be("node1");
            nodes[0].IndexName.Should().Be("name");
            nodes[0].Value.Should().Be("marko2");
        });
    }

    [Fact]
    public void UpdateIndexOnOneNodeIndexCarryForward()
    {
        var map = new GraphMap()
        {
            new GraphNode("node2", tags: "name=vadas,age=27"),
            new GraphNode("node1", tags: "name=marko,age=29", indexes: "name"),
        };

        map.Nodes.LookupIndex("name", "marko").Action(subject =>
        {
            subject.IsOk().Should().BeTrue();
            subject.Return().NodeKey.Should().Be("node1");
        });

        var updatedNode = new GraphNode("node1", tags: "name=marko2,age=29");
        var setResult = map.Nodes.Set(updatedNode);
        setResult.IsOk().Should().BeTrue();

        map.Nodes.TryGetValue("node1", out var readNode).Action(x =>
        {
            x.Should().BeTrue();
            readNode!.Key.Should().Be("node1");
            readNode.TagsString.Should().Be("age=29,name=marko2");
            readNode.IndexesString.Should().Be("name");
        });

        map.Nodes.LookupIndex("name", "marko").IsError().Should().BeTrue();

        map.Nodes.LookupIndex("name", "marko2").Action(subject =>
        {
            subject.IsOk().Should().BeTrue();
            subject.Return().NodeKey.Should().Be("node1");
        });

        map.Nodes.LookupByNodeKey("node1").Action(nodes =>
        {
            nodes.Count.Should().Be(1);
            nodes[0].NodeKey.Should().Be("node1");
            nodes[0].IndexName.Should().Be("name");
            nodes[0].Value.Should().Be("marko2");
        });
    }


    [Fact]
    public void RemoveIndexFromMultipleIndexes()
    {
        var map = new GraphMap()
        {
            new GraphNode("node2", tags: "name=vadas,age=27"),
            new GraphNode("node1", tags: "name=marko,age=29", indexes: "name"),
            new GraphNode("node5", tags: "name=ripple,lang=java", indexes: "lang, name"),
        };

        map.Nodes.LookupIndex("name", "marko").Action(x =>
        {
            x.IsOk().Should().BeTrue();
            x.Return().NodeKey.Should().Be("node1");
        });

        map.Nodes.LookupIndex("name", "ripple").Action(x =>
        {
            x.IsOk().Should().BeTrue();
            x.Return().NodeKey.Should().Be("node5");
        });

        map.Nodes.LookupIndex("lang", "java").Action(x =>
        {
            x.IsOk().Should().BeTrue();
            x.Return().NodeKey.Should().Be("node5");
        });

        map.Nodes.LookupByNodeKey("node1").Action(nodes =>
        {
            nodes.Count.Should().Be(1);
            nodes[0].NodeKey.Should().Be("node1");
            nodes[0].IndexName.Should().Be("name");
            nodes[0].Value.Should().Be("marko");
        });

        map.Nodes.LookupByNodeKey("node5").Action(nodes =>
        {
            nodes.Count.Should().Be(2);
            nodes.Count(x => x.IndexName == "name" && x.Value == "ripple").Should().Be(1);
            nodes.Count(x => x.IndexName == "lang" && x.Value == "java").Should().Be(1);
        });

        var updatedNode = new GraphNode("node5", tags: "-lang");
        var setResult = map.Nodes.Set(updatedNode);
        setResult.IsOk().Should().BeTrue();

        map.Nodes.LookupIndex("lang", "java").IsNotFound().Should().BeTrue();

        map.Nodes.LookupIndex("name", "marko").Action(x =>
        {
            x.IsOk().Should().BeTrue();
            x.Return().NodeKey.Should().Be("node1");
        });

        map.Nodes.LookupIndex("name", "ripple").Action(x =>
        {
            x.IsOk().Should().BeTrue();
            x.Return().NodeKey.Should().Be("node5");
        });

        map.Nodes.LookupByNodeKey("node5").Action(nodes =>
        {
            nodes.Count.Should().Be(1);
            nodes.Count(x => x.IndexName == "name" && x.Value == "ripple").Should().Be(1);
            nodes.Count(x => x.IndexName == "lang" && x.Value == "java").Should().Be(0);
        });

        map.Nodes.TryGetValue("node5", out var readNode).Should().BeTrue();
        readNode.NotNull();
        readNode!.Key.Should().Be("node5");
        readNode.TagsString.Should().Be("name=ripple");
    }

    [Fact]
    public void RemoveNameIndexFromMultipleIndexes()
    {
        var map = new GraphMap()
        {
            new GraphNode("node2", tags: "name=vadas,age=27"),
            new GraphNode("node1", tags: "name=marko,age=29", indexes: "name"),
            new GraphNode("node5", tags: "name=ripple,lang=java", indexes: "lang, name"),
        };

        map.Nodes.LookupIndex("name", "marko").Action(x =>
        {
            x.IsOk().Should().BeTrue();
            x.Return().NodeKey.Should().Be("node1");
        });

        map.Nodes.LookupIndex("name", "ripple").Action(x =>
        {
            x.IsOk().Should().BeTrue();
            x.Return().NodeKey.Should().Be("node5");
        });

        map.Nodes.LookupIndex("lang", "java").Action(x =>
        {
            x.IsOk().Should().BeTrue();
            x.Return().NodeKey.Should().Be("node5");
        });

        map.Nodes.LookupByNodeKey("node1").Action(nodes =>
        {
            nodes.Count.Should().Be(1);
            nodes[0].NodeKey.Should().Be("node1");
            nodes[0].IndexName.Should().Be("name");
            nodes[0].Value.Should().Be("marko");
        });

        map.Nodes.LookupByNodeKey("node5").Action(nodes =>
        {
            nodes.Count.Should().Be(2);
            nodes.Count(x => x.IndexName == "name" && x.Value == "ripple").Should().Be(1);
            nodes.Count(x => x.IndexName == "lang" && x.Value == "java").Should().Be(1);
        });

        var updatedNode = new GraphNode("node5", tags: "-name, -lang");
        var setResult = map.Nodes.Set(updatedNode);
        setResult.IsOk().Should().BeTrue();

        map.Nodes.LookupIndex("lang", "java").IsNotFound().Should().BeTrue();
        map.Nodes.LookupIndex("name", "ripple").IsNotFound().Should().BeTrue();

        map.Nodes.LookupByNodeKey("node5").Action(nodes =>
        {
            nodes.Count.Should().Be(0);
            nodes.Count(x => x.IndexName == "name" && x.Value == "ripple").Should().Be(0);
            nodes.Count(x => x.IndexName == "lang" && x.Value == "java").Should().Be(0);
        });

        map.Nodes.TryGetValue("node5", out var readNode).Should().BeTrue();
        readNode.NotNull();
        readNode!.Key.Should().Be("node5");
        readNode.Tags.Count.Should().Be(0);
    }

}
