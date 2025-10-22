using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Frozen;
using Toolbox.Extensions;
using Toolbox.Graph;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph.test.Graph;

public class GraphNodeGrantsTests
{
    private static IReadOnlyDictionary<string, string?> _emptyTags = FrozenDictionary<string, string?>.Empty;
    private static IReadOnlyDictionary<string, GraphLink> _emptyData = FrozenDictionary<string, GraphLink>.Empty;

    [Fact]
    public void StringCtor_ParsesGrants_Dedupes_AndBuildsSet()
    {
        var node = new GraphNode(
            key: "k",
            grants: "ni:o:pi:user1, ni:r:sg:group1, ni:o:pi:user1" // duplicate g1
        );

        var g1 = new GrantPolicy("ni", RolePolicy.Owner | RolePolicy.PrincipalIdentity, "user1");
        var g2 = new GrantPolicy("ni", RolePolicy.Reader | RolePolicy.SecurityGroup, "group1");

        node.Grants.Count.Be(2);
        node.Grants.Contains(g1).BeTrue();
        node.Grants.Contains(g2).BeTrue();
    }

    [Fact]
    public void Equality_IgnoresOrder_ForGrants()
    {
        var n1 = new GraphNode(
            key: "k",
            tags: "t1,t2=v2",
            indexes: "a,b",
            foreignKeys: "fk=v",
            grants: "ni:o:pi:user1, ni:r:sg:group1"
        );

        var g1 = new GrantPolicy("ni", RolePolicy.Owner | RolePolicy.PrincipalIdentity, "user1");
        var g2 = new GrantPolicy("ni", RolePolicy.Reader | RolePolicy.SecurityGroup, "group1");

        var n2 = new GraphNode(
            key: "k",
            tags: "t1,t2=v2".ToTags(),
            createdDate: n1.CreatedDate,                // ensure CreatedDate matches for equality
            dataMap: _emptyData,
            indexes: new[] { "b", "a" },               // different order
            foreignKeys: "fk=v".ToTags(),
            grants: new[] { g2, g1 }                   // reversed order
        );

        (n1 == n2).BeTrue();
    }

    [Fact]
    public void Inequality_WhenGrantsDiffer()
    {
        var created = DateTime.UtcNow;

        var g1 = new GrantPolicy("ni", RolePolicy.Owner | RolePolicy.PrincipalIdentity, "user1");
        var g2 = new GrantPolicy("ni", RolePolicy.Reader | RolePolicy.SecurityGroup, "group1");

        var n1 = new GraphNode(
            key: "k",
            tags: "t1".ToTags(),
            createdDate: created,
            dataMap: _emptyData,
            indexes: FrozenSet<string>.Empty,
            foreignKeys: _emptyTags,
            grants: new[] { g1, g2 }
        );

        var n2 = new GraphNode(
            key: "k",
            tags: "t1".ToTags(),
            createdDate: created,
            dataMap: _emptyData,
            indexes: FrozenSet<string>.Empty,
            foreignKeys: _emptyTags,
            grants: new[] { g1 } // missing g2
        );

        (n1 == n2).BeFalse();
    }

    [Fact]
    public void Serialization_RoundTrip_IncludesGrants()
    {
        var created = DateTime.UtcNow;

        var node = new GraphNode(
            key: "node1",
            tags: new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
            {
                ["t1"] = null,
                ["t2"] = "v2",
            },
            createdDate: created,
            dataMap: new Dictionary<string, GraphLink>(StringComparer.OrdinalIgnoreCase)
            {
                ["doc"] = new GraphLink { NodeKey = "n:1", Name = "readme", FileId = "/a/readme.md" },
            },
            indexes: new[] { "a", "b" },
            foreignKeys: new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
            {
                ["fk1"] = "v1",
            },
            grants: new []
            {
                new GrantPolicy("ni1", RolePolicy.Owner | RolePolicy.PrincipalIdentity, "user1"),
                new GrantPolicy("ni2", RolePolicy.Contributor | RolePolicy.SecurityGroup, "groupA"),
            }
        );

        string json = node.ToJson();
        var read = json.ToObject<GraphNode>().NotNull();

        (node == read).BeTrue();
    }

    [Fact]
    public void JsonCtor_Equals_StringCtor_ForSameGrants()
    {
        var g1 = new GrantPolicy("ni", RolePolicy.Owner | RolePolicy.PrincipalIdentity, "user1");
        var g2 = new GrantPolicy("edge1", RolePolicy.Contributor | RolePolicy.PrincipalIdentity, "user2");

        var n1 = new GraphNode(
            key: "k",
            grants: "ni:o:pi:user1, edge1:c:pi:user2"
        );

        var n2 = new GraphNode(
            key: "k",
            tags: n1.Tags,
            createdDate: n1.CreatedDate,
            dataMap: _emptyData,
            indexes: n1.Indexes,
            foreignKeys: n1.ForeignKeys,
            grants: new[] { g2, g1 } // different order
        );

        (n1 == n2).BeTrue();
    }

    [Fact]
    public void StringCtor_InvalidGrant_Throws()
    {
        // Missing parts / invalid schema-role tokens
        Assert.Throws<ArgumentException>(() => new GraphNode("k", grants: "badformat"));
        Assert.Throws<ArgumentException>(() => new GraphNode("k", grants: "ni:o:user1"));     // only 2 colons
        Assert.Throws<ArgumentException>(() => new GraphNode("k", grants: "ni:x:xx:user1")); // invalid role/schema tokens
    }
}
