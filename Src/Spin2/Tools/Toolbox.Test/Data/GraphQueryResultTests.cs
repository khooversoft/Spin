using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Toolbox.Data;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Test.Data;

public class GraphQueryResultTests
{
    [Fact]
    public void Serialization()
    {
        var nodes = new List<IGraphCommon>
        {
            new GraphNode("node1", "t1"),
            new GraphNode("node2", "t2"),
            new GraphNode("node3", "t3;t4=v1"),
        };

        var edges = new List<IGraphCommon>
        {
            new GraphEdge("from1", "to1", "edge1", "p1"),
            new GraphEdge("from2", "to2", "edge2", "p2"),
            new GraphEdge("from3", "to3", "edge3", "p3"),
            new GraphEdge("from4", "to4", "edge4", "p4=t3"),
        };

        var v = new GraphQueryResult
        {
            StatusCode = StatusCode.OK,
            Error = "no error",
            Items = nodes.Concat(edges).ToArray(),
            Alias = new Dictionary<string, IReadOnlyList<IGraphCommon>>
            {
                ["a1"] = nodes,
                ["b2"] = edges,
            },
        };

        string json = v.ToJson();

        GraphQueryResult result = json.ToObject<GraphQueryResult>().NotNull();
        result.Items.Count.Should().Be(nodes.Count + edges.Count);
        result.Alias.Count.Should().Be(2);

        var n = result.Items.OfType<GraphNode>().OrderBy(x => x.Key).ToArray();
        Enumerable.SequenceEqual(nodes, n).Should().BeTrue();

        var e = result.Items.OfType<GraphEdge>().OrderBy(x => x.FromKey).ToArray();
        Enumerable.SequenceEqual(edges, e).Should().BeTrue();

        result.Alias.ContainsKey("a1").Should().BeTrue();
        var n1 = result.Alias["a1"].OfType<GraphNode>().OrderBy(x => x.Key).ToArray();
        Enumerable.SequenceEqual(nodes, n1).Should().BeTrue();

        result.Alias.ContainsKey("b2").Should().BeTrue();
        var e1 = result.Alias["b2"].OfType<GraphEdge>().OrderBy(x => x.FromKey).ToArray();
        Enumerable.SequenceEqual(edges, e1).Should().BeTrue();
    }
}
