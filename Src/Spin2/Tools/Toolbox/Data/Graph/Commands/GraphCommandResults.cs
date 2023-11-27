using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Data;

public record GraphCommandResults
{
    public IReadOnlyList<GraphQueryResult> Items { get; init; } = Array.Empty<GraphQueryResult>();
}


//public record GraphCommandResult
//{
//    public GraphCommandResult() { }

//    public GraphCommandResult(CommandType commandType, Option option)
//    {
//        CommandType = commandType;
//        StatusCode = option.StatusCode;
//        Error = option.Error;
//    }

//    public GraphCommandResult(CommandType commandType, GraphQueryResult queryResult)
//    {
//        CommandType = commandType;
//        StatusCode = StatusCode.OK;
//        SearchResult = queryResult.NotNull();
//    }

//    public CommandType CommandType { get; init; }
//    public StatusCode StatusCode { get; init; }
//    public string? Error { get; init; }
//    public GraphQueryResult SearchResult { get; init; } = new GraphQueryResult();
//}


public static class GraphCommandResultsExtensions
{
    //public static bool IsOk(this GraphCommandResult subject) => subject.NotNull().StatusCode.IsOk();
    //public static bool IsError(this GraphCommandResult subject) => subject.NotNull().StatusCode.IsError();

    public static bool IsOk(this GraphCommandResults subject) => subject.NotNull().Items.All(x => x.StatusCode.IsOk());
    public static bool IsError(this GraphCommandResults subject) => subject.NotNull().Items.Any(x => x.StatusCode.IsError());

    //public static GraphQueryResult SingleQueryResult(this GraphCommandResults subject)
    //{
    //    subject.NotNull();
    //    subject.Items.Count.Assert(x => x == 1, $"Items count is not one, count={subject.Items.Count}");

    //    var result = subject.Items[0].StatusCode switch
    //    {
    //        StatusCode.NoContent => new GraphQueryResult(),
    //        StatusCode.OK => subject.Items[0],

    //        _ => throw new ArgumentException("Failed status"),
    //    };

    //    return result;
    //}
}
