﻿using FluentAssertions;
using Toolbox.Data;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Test.Data.Types;

public class GraphCommandResultSerialization
{
    [Fact]
    public void SimpleSerialization()
    {
        GraphCommandResult source = new GraphCommandResult
        {
            CommandType = CommandType.AddEdge,
            StatusCode = StatusCode.Conflict,
            Error = "error",
        };

        string json = source.ToJson();

        GraphCommandResult result = json.ToObject<GraphCommandResult>().NotNull();

        result.CommandType.Should().Be(CommandType.AddEdge);
        result.StatusCode.Should().Be(StatusCode.Conflict);
        result.Error.Should().Be("error");
    }

    [Fact]
    public void WithResultSerialization()
    {
        GraphCommandResult source = new GraphCommandResult
        {
            CommandType = CommandType.AddEdge,
            StatusCode = StatusCode.Conflict,
            Error = "error",
            SearchResult = new GraphQueryResult
            {
                StatusCode = StatusCode.OK,
                Error = "no error",
                Items = new IGraphCommon[]
                {
                    new GraphNode("key1"),
                    new GraphEdge("key1", "key2"),
                },
            },
        };

        string json = source.ToJson();

        GraphCommandResult result = json.ToObject<GraphCommandResult>().NotNull();
    }
}
