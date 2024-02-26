using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Data;

public enum CommandType
{
    None,
    AddNode,
    AddEdge,
    UpdateEdge,
    UpdateNode,
    DeleteEdge,
    DeleteNode,
    Select,
}


public record GraphQueryResult
{
    public GraphQueryResult() { }
    public GraphQueryResult(CommandType commandType, StatusCode statusCode, string? error = null) => (CommandType, StatusCode, Error) = (commandType, statusCode, Error);
    public GraphQueryResult(CommandType commandType, IEnumerable<IGraphCommon> items) => (CommandType, Items) = (commandType, items.NotNull().ToArray());

    public StatusCode StatusCode { get; init; }
    public string? Error { get; init; }
    public CommandType? CommandType { get; init; }

    public IReadOnlyList<IGraphCommon> Items { get; init; } = Array.Empty<IGraphCommon>();
    public IReadOnlyDictionary<string, IReadOnlyList<IGraphCommon>> Alias { get; init; } = new Dictionary<string, IReadOnlyList<IGraphCommon>>(StringComparer.OrdinalIgnoreCase);
}

public record GraphQueryResults
{
    public IReadOnlyList<GraphQueryResult> Items { get; init; } = Array.Empty<GraphQueryResult>();
}


public static class GraphQueryResultExtensions
{
    public static IReadOnlyList<GraphEdge> Edges(this GraphQueryResult subject) => subject.NotNull().Items.OfType<GraphEdge>().ToArray();
    public static IReadOnlyList<GraphNode> Nodes(this GraphQueryResult subject) => subject.NotNull().Items.OfType<GraphNode>().ToArray();

    public static IReadOnlyList<GraphEdge> AliasEdge(this GraphQueryResult subject, string key) => subject.NotNull().Alias[key].OfType<GraphEdge>().ToArray();
    public static IReadOnlyList<GraphNode> AliasNode(this GraphQueryResult subject, string key) => subject.NotNull().Alias[key].OfType<GraphNode>().ToArray();

    public static bool IsOk(this GraphQueryResult subject) => subject.NotNull().StatusCode.IsOk();
    public static bool IsError(this GraphQueryResult subject) => subject.NotNull().StatusCode.IsError();
}