using System.Collections.Frozen;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph.test.Graph;

public class NodeEqualsTests
{
    private static IReadOnlyDictionary<string, string?> _emptyTags = FrozenDictionary<string, string?>.Empty;
    private static IReadOnlyDictionary<string, GraphLink> _emptyData = FrozenDictionary<string, GraphLink>.Empty;

    [Fact]
    public void NodeEqual()
    {
        DateTime now = DateTime.UtcNow;

        var node1 = new GraphNode(
            "node1",
            tags: "t1,t2=v2".ToTags(),
            createdDate: now,
            dataMap: _emptyData,
            indexes: ["t1"],
            foreignKeys: "t1".ToTags()
            );

        var node2 = new GraphNode(
            "node1",
            tags: "t1,t2=v2".ToTags(),
            createdDate: now,
            dataMap: _emptyData,
            indexes: ["t1"],
            foreignKeys: "t1".ToTags()
            );

        (node1 == node2).BeTrue();

    }

    [Fact]
    public void NodeNotEqual()
    {
        DateTime now = DateTime.UtcNow;

        var node1 = new GraphNode(
            "node1",
            tags: "t1,t2=v2".ToTags(),
            createdDate: now,
            dataMap: _emptyData,
            indexes: ["t1"],
            foreignKeys: "t1".ToTags()
            );

        var node2 = new GraphNode(
            "node1",
            tags: "t1,t2=v2".ToTags(),
            createdDate: now,
            dataMap: _emptyData,
            indexes: ["t2"],
            foreignKeys: "t1".ToTags()
            );

        (node1 == node2).BeFalse();

        var node3 = new GraphNode(
            "node1",
            tags: "t1,t2=v2".ToTags(),
            createdDate: now,
            dataMap: _emptyData,
            indexes: ["t1"],
            foreignKeys: "t2".ToTags()
            );

        (node1 == node3).BeFalse();
    }
}
