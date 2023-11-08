using Toolbox.Types;

namespace Toolbox.Data.Graph.Execute;

public enum CommandType
{
    AddNode,
    AddEdge,
    UpdateEdge,
    UpdateNode,
    DeleteEdge,
    DeleteNode,
    Select,
}

public record CommandResult
{
    public CommandResult() { }

    public CommandResult(CommandType commandType, Option option)
    {
        CommandType = commandType;
        StatusCode = option.StatusCode;
        Error = option.Error;
    }

    public CommandResult(CommandType commandType, IEnumerable<IGraphCommon> searchResults)
    {
        CommandType = commandType;
        StatusCode = StatusCode.OK;
        SearchResult = searchResults.ToArray();
    }

    public CommandType CommandType { get; init; }
    public StatusCode StatusCode { get; init; }
    public string? Error { get; init; }
    public IReadOnlyList<IGraphCommon> SearchResult { get; init; } = Array.Empty<IGraphCommon>();
}

public record CommandResponse
{
}
