//using System.Collections.Immutable;
//using FluentAssertions;
//using Toolbox.Data;
//using Toolbox.Extensions;
//using Toolbox.Tools;
//using Toolbox.Types;

//namespace Toolbox.Graph.test.Graph.Query;

//public class GraphCommandResultSerialization
//{
//    [Fact]
//    public void SimpleSerialization()
//    {
//        GraphQueryResult source = new GraphQueryResult
//        {
//            CommandType = CommandType.AddEdge,
//            Status = (StatusCode.Conflict, "error"),
//        };

//        string json = source.ToJson();

//        GraphQueryResult result = json.ToObject<GraphQueryResult>().NotNull();

//        result.CommandType.Should().Be(CommandType.AddEdge);
//        result.Status.StatusCode.Should().Be(StatusCode.Conflict);
//        result.Status.Error.Should().Be("error");
//    }

//    [Fact]
//    public void WithResultSerialization()
//    {
//        GraphQueryResult source = new GraphQueryResult
//        {
//            Status = (StatusCode.OK, "no error"),
//            Items = new IGraphCommon[]
//            {
//                new GraphNode("key1"),
//                new GraphEdge("key1", "key2"),
//            }.ToImmutableArray(),
//        };

//        string json = source.ToJson();

//        GraphQueryResult result = json.ToObject<GraphQueryResult>().NotNull();
//    }
//}
