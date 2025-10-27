using Toolbox.Extensions;
using Toolbox.Tools;

namespace Toolbox.Graph.test.Graph.Map;

public class GraphMapSerializationTests
{
    [Fact]
    public void EmptyMapString()
    {
        var map = new GraphMap();

        var json = map.ToJson();

        var mapResult = GraphMapTool.FromJson(json).NotNull();
        mapResult.NotNull();
        mapResult.Count().Be(0);
        mapResult.Nodes.Count.Be(0);
        mapResult.Edges.Count.Be(0);
    }

    [Fact]
    public void SingleNodeMap()
    {
        new GraphMap()
        {
            new GraphNode("Node1"),
        }.Action(x =>
        {
            var json = x.ToJson();

            var mapResult = GraphMapTool.FromJson(json).NotNull();
            mapResult.NotNull();
            mapResult.Count().Be(1);
            mapResult.Edges.Count.Be(0);

            mapResult.Nodes.Count.Be(1);
            mapResult.Nodes.First().Action(y =>
            {
                y.Key.Be("Node1");
                y.Tags.NotNull();
                y.Tags.Count.Be(0);
            });
        });

        new GraphMap()
        {
            new GraphNode("Node1"),
        }.Action(x =>
        {
            var json = x.ToJson();

            var mapResult = GraphMapTool.FromJson(json).NotNull();
            mapResult.NotNull();
            mapResult.Count().Be(1);
            mapResult.Edges.Count.Be(0);

            mapResult.Nodes.Count.Be(1);
            mapResult.Nodes.First().Action(y =>
            {
                y.Key.Be("Node1");
                y.Tags.NotNull();
                y.Tags.Count.Be(0);
            });
        });

        new GraphMap()
        {
            new GraphNode("Node1", tags: "t1,t2=v2"),
        }.Action(x =>
        {
            var json = x.ToJson();

            var mapResult = GraphMapTool.FromJson(json).NotNull();
            mapResult.NotNull();
            mapResult.Count().Be(1);
            mapResult.Edges.Count.Be(0);

            mapResult.Nodes.Count.Be(1);
            mapResult.Nodes.First().Action(y =>
            {
                y.Key.Be("Node1");
                y.Tags.NotNull();
                y.Tags.Count.Be(2);
                y.Tags["t1"].BeNull();
                y.Tags["t2"].Be("v2");
            });
        });

        new GraphMap()
        {
            new GraphNode("Node1", tags: "t1,t2=v2"),
        }.Action(x =>
        {
            var json = x.ToJson();

            var mapResult = GraphMapTool.FromJson(json).NotNull();
            mapResult.NotNull();
            mapResult.Count().Be(1);
            mapResult.Edges.Count.Be(0);

            mapResult.Nodes.Count.Be(1);
            mapResult.Nodes.First().Action(y =>
            {
                y.Key.Be("Node1");
                y.Tags.NotNull();
                y.Tags.Count.Be(2);
                y.Tags["t1"].BeNull();
                y.Tags["t2"].Be("v2");
            });
        });
    }

    [Fact]
    public void GraphMapSample1()
    {
        var map = new GraphMap()
        {
            new GraphNode("node1"),
            new GraphNode("node2"),
            new GraphNode("node3"),
            new GraphNode("node4"),

            new GraphEdge("node1", "node2", "et"),
            new GraphEdge("node1", "node3", "et"),
            new GraphEdge("node1", "node4", "et"),
        };

        string json = map.ToJson();

        var mapRead = GraphMapTool.FromJson(json).NotNull();
        mapRead.Nodes.Count.Be(4);
        mapRead.Edges.Count.Be(3);

        var s1 = map.Nodes.Select(x => x.Key).OrderBy(x => x).ToArray();
        var s2 = mapRead.Nodes.Select(x => x.Key).OrderBy(x => x).ToArray();
        s1.SequenceEqual(s2).BeTrue();

        var e1 = map.Edges.Select(x => x.ToString()).OrderBy(x => x).ToArray();
        var e2 = mapRead.Edges.Select(x => x.ToString()).OrderBy(x => x).ToArray();
        e1.SequenceEqual(e2).BeTrue();
    }

    [Fact]
    public void Edge_WithTags_And_CreatedDate_RoundTrip()
    {
        var created = new DateTime(2024, 05, 01, 10, 30, 00, DateTimeKind.Utc);

        var map = new GraphMap()
        {
            new GraphNode("N1"),
            new GraphNode("N2"),
            new GraphEdge("N1", "N2", "et-main", tags: "k1=v1,k2", createdDate: created),
        };

        var json = map.ToJson();
        var mapResult = GraphMapTool.FromJson(json).NotNull();

        mapResult.Edges.Count.Be(1);
        mapResult.Edges.First().Action(e =>
        {
            e.FromKey.Be("N1");
            e.ToKey.Be("N2");
            e.EdgeType.Be("et-main");
            e.Tags.Count.Be(2);
            e.Tags["k1"].Be("v1");
            e.Tags["k2"].BeNull();
            e.CreatedDate.Be(created);
        });
    }

    [Fact]
    public void Node_FullPayload_RoundTrip()
    {
        var created = new DateTime(2024, 01, 02, 03, 04, 05, DateTimeKind.Utc);

        var dataMap = new Dictionary<string, GraphLink>(StringComparer.OrdinalIgnoreCase)
        {
            ["doc"] = new GraphLink { NodeKey = "N1", Name = "doc", FileId = "a/b" },
        };

        var tags = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase) { ["t1"] = "v1", ["t2"] = null };
        var indexes = new[] { "ix1", "IX2" };
        var foreignKeys = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase) { ["fk1"] = "ref1" };
        var grants = new[]
        {
            new GrantPolicy("N1", RolePolicy.Owner | RolePolicy.PrincipalIdentity, "user:alice"),
            new GrantPolicy("N1", RolePolicy.Reader | RolePolicy.SecurityGroup, "sg:devs"),
        };

        var node = new GraphNode("N1", tags, created, dataMap, indexes, foreignKeys, grants);

        var map = new GraphMap() { node };

        var json = map.ToJson();
        var mapResult = GraphMapTool.FromJson(json).NotNull();

        mapResult.Nodes.Count.Be(1);
        var n = mapResult.Nodes.First();
        (n == node).BeTrue(); // uses GraphNode.Equals (covers tags, createdDate, dataMap, indexes, foreignKeys, grants)
    }

    [Fact]
    public void RoundTrip_LastLogSequenceNumber_And_GrantControl()
    {
        var map = new GraphMap();
        map.SetLastLogSequenceNumber("lsn-123");

        var alice = new PrincipalIdentity("pi:alice", "alice", "alice@contoso.com", emailConfirmed: true);
        map.GrantControl.Principals.Add(alice);

        var devs = new GroupPolicy("sg:devs", new[] { alice.PrincipalId });
        map.GrantControl.Groups.Add(devs);

        var json = map.ToJson();
        var mapResult = GraphMapTool.FromJson(json).NotNull();

        mapResult.LastLogSequenceNumber.Be("lsn-123");
        mapResult.GrantControl.Principals.Count.Be(1);
        mapResult.GrantControl.Groups.Count.Be(1);

        mapResult.GrantControl.Principals.TryGetByNameIdentifier("pi:alice", out var alice2).BeTrue();
        alice2.Email.Be("alice@contoso.com");

        mapResult.GrantControl.Groups.Contains("sg:devs").BeTrue();
        mapResult.GrantControl.Groups.TryGetGroup("sg:devs", out var g).BeTrue();
        g.Members.Contains(alice.PrincipalId).BeTrue();
    }

    [Fact]
    public void SourceGen_SerializeMap_DeserializeMap_RoundTrip()
    {
        var map = new GraphMap()
        {
            new GraphNode("A", tags: "k=v"),
            new GraphNode("B"),
            new GraphEdge("A", "B", "et"),
        };
        map.SetLastLogSequenceNumber("lsn-999");

        var json = map.SerializeMap();
        var map2 = GraphSerializationTool.DeserializeMap(json).NotNull();

        map2.Nodes.Count.Be(2);
        map2.Edges.Count.Be(1);
        map2.LastLogSequenceNumber.Be("lsn-999");
    }

    [Fact]
    public void FromJson_Invalid_Throws()
    {
        var badJson = "{ notValid: true ";
        Assert.ThrowsAny<Exception>(() => GraphMapTool.FromJson(badJson));
    }
}
