using FluentAssertions;
using Toolbox.Data;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Test.Data.Graph.Command;

public class GraphCommandSerializationTest
{
    [Fact]
    public void GraphCommandResults()
    {
        var g = new GraphCommandResults
        {
            Items = new[]
            {
                new GraphCommandResult(CommandType.Select, new Option(StatusCode.OK)),
            },
        };

        string json = Json.Default.SerializePascal(g);

        GraphCommandResults r = json.ToObject<GraphCommandResults>().NotNull();
        r.Should().NotBeNull();
        r.Items.Count.Should().Be(1);
        r.Items[0].CommandType.Should().Be(CommandType.Select);
    }

    [Fact]
    public void SampleDeserialization()
    {
        string json = """
            {"items":[{"commandType":5,"statusCode":0,"error":null,"searchResult":null}]}
            """;

        GraphCommandResults r = json.ToObject<GraphCommandResults>().NotNull();
        r.Should().NotBeNull();
        r.Items.Count.Should().Be(1);
        r.Items[0].CommandType.Should().Be(CommandType.DeleteNode);
    }

    [Fact]
    public void GraphCommandResultsWithResult()
    {
        var g = new GraphCommandResults
        {
            Items = new[]
            {
                new GraphCommandResult(CommandType.Select, new Option(StatusCode.OK))
                {
                    SearchResult = new GraphQueryResult
                    {
                        StatusCode = StatusCode.OK,
                        Items = new IGraphCommon[]
                        {
                            new GraphEdge("fromKey1", "toKey1", "edgeType2"),
                            new GraphNode("key1", "t1"),
                        }
                    },
                },
            },
        };

        string json = g.ToJson();

        GraphCommandResults r = json.ToObject<GraphCommandResults>().NotNull();
        r.Should().NotBeNull();
        r.Items.Count.Should().Be(1);
        r.Items[0].CommandType.Should().Be(CommandType.Select);
        r.Items[0].SearchResult.Should().NotBeNull();
        r.Items[0].SearchResult!.StatusCode.Should().Be(StatusCode.OK);
        r.Items[0].SearchResult!.Items.Count.Should().Be(2);

        r.Items[0].SearchResult!.Items[0].Cast<GraphEdge>().Action(x =>
        {
            x.FromKey.Should().Be("fromKey1");
            x.ToKey.Should().Be("toKey1");
            x.EdgeType.Should().Be("edgeType2");
        });
        r.Items[0].SearchResult!.Items[1].Cast<GraphNode>().Action(x =>
        {
            x.Key.Should().Be("key1");
            x.Tags.ToString().Should().Be("t1");
        });
    }
}
