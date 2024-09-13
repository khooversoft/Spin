﻿//using System.Collections.Immutable;
//using FluentAssertions;
//using Toolbox.Data;
//using Toolbox.Extensions;
//using Toolbox.Tools;
//using Toolbox.Types;

//namespace Toolbox.Graph.test.Graph.Query;

//public class GraphQueryResultTests
//{
//    [Fact]
//    public void Serialization()
//    {
//        var nodes = new List<IGraphCommon>
//        {
//            new GraphNode("node1", "t1"),
//            new GraphNode("node2", "t2"),
//            new GraphNode("node3", "t3,t4=v1"),
//        };

//        var edges = new List<IGraphCommon>
//        {
//            new GraphEdge("from1", "to1", "edge1", "p1"),
//            new GraphEdge("from2", "to2", "edge2", "p2"),
//            new GraphEdge("from3", "to3", "edge3", "p3"),
//            new GraphEdge("from4", "to4", "edge4", "p4=t3"),
//        };

//        var v = new GraphQueryResult
//        {
//            Status = (StatusCode.OK, "no error"),
//            Items = nodes.Concat(edges).ToImmutableArray(),
//            Alias = new Dictionary<string, IReadOnlyList<IGraphCommon>>
//            {
//                ["a1"] = nodes,
//                ["b2"] = edges,
//            }.ToImmutableDictionary(x => x.Key, x => x.Value.ToImmutableArray()),
//        };

//        string json = v.ToJson();

//        GraphQueryResult result = json.ToObject<GraphQueryResult>().NotNull();
//        result.Items.Count.Should().Be(nodes.Count + edges.Count);
//        result.Alias.Count.Should().Be(2);

//        var n = result.Items.OfType<GraphNode>().OrderBy(x => x.Key).ToArray();
//        nodes.SequenceEqual(n).Should().BeTrue();

//        var e = result.Items.OfType<GraphEdge>().OrderBy(x => x.FromKey).ToArray();
//        edges.SequenceEqual(e).Should().BeTrue();

//        result.Alias.ContainsKey("a1").Should().BeTrue();
//        var n1 = result.Alias["a1"].OfType<GraphNode>().OrderBy(x => x.Key).ToArray();
//        nodes.SequenceEqual(n1).Should().BeTrue();

//        result.Alias.ContainsKey("b2").Should().BeTrue();
//        var e1 = result.Alias["b2"].OfType<GraphEdge>().OrderBy(x => x.FromKey).ToArray();
//        edges.SequenceEqual(e1).Should().BeTrue();
//    }
//}
