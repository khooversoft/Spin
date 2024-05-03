﻿using System.Collections.Immutable;
using FluentAssertions;
using Toolbox.Data;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph.test.Command;

public class GraphCommandSerializationTests
{
    [Fact]
    public void GraphCommandResults()
    {
        var g = new GraphQueryResults
        {
            Items = new[]
            {
                new GraphQueryResult(CommandType.Select, StatusCode.OK),
            }.ToImmutableArray(),
        };

        string json = Json.Default.SerializePascal(g);

        GraphQueryResults r = json.ToObject<GraphQueryResults>().NotNull();
        r.Should().NotBeNull();
        r.Items.Length.Should().Be(1);
        r.Items[0].CommandType.Should().Be(CommandType.Select);
    }

    [Fact]
    public void GraphCommandResultsWithResult()
    {
        ImmutableArray<IGraphCommon> items = new IGraphCommon[]
        {
            new GraphEdge("fromKey1", "toKey1", "edgeType2"),
            new GraphNode("key1", "t1"),
        }.ToImmutableArray<IGraphCommon>();


        var g = new GraphQueryResults
        {
            Items = new[]
            {
                new GraphQueryResult
                {
                    CommandType = CommandType.Select,
                    Status = StatusCode.OK,
                    Items = new IGraphCommon[]
                    {
                        new GraphEdge("fromKey1", "toKey1", "edgeType2"),
                        new GraphNode("key1", "t1"),
                    }.ToImmutableArray(),
                },
            }.ToImmutableArray(),
        };

        string json = g.ToJson();

        GraphQueryResults r = json.ToObject<GraphQueryResults>().NotNull();
        r.Should().NotBeNull();
        r.Items.Length.Should().Be(1);
        r.Items[0].CommandType.Should().Be(CommandType.Select);
        r.Items[0].Should().NotBeNull();
        r.Items[0].Status.StatusCode.Should().Be(StatusCode.OK);
        r.Items[0].Items.Length.Should().Be(2);

        r.Items[0].Items[0].Cast<GraphEdge>().Action(x =>
        {
            x.FromKey.Should().Be("fromKey1");
            x.ToKey.Should().Be("toKey1");
            x.EdgeType.Should().Be("edgeType2");
        });
        r.Items[0].Items[1].Cast<GraphNode>().Action(x =>
        {
            x.Key.Should().Be("key1");
            x.Tags.ToTagsString().Should().Be("t1");
        });
    }
}
