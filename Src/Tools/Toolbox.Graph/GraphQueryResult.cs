using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

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
    public GraphQueryResult(CommandType commandType, Option status) => (CommandType, Status) = (commandType, status);

    public Option Status { get; init; }
    //public StatusCode StatusCode { get; init; }
    //public string? Error { get; init; }
    public CommandType? CommandType { get; init; }
    public override string ToString() => $"{Status}, {nameof(CommandType)}={CommandType}";

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

    //public static bool IsOk(this GraphQueryResult subject) => subject.NotNull().Status.IsOk();
    //public static bool IsError(this GraphQueryResult subject) => subject.NotNull().StatusCode.IsError();

    public static bool HasScalarResult(this GraphQueryResults subject) => subject.NotNull().Items.Count == 1 && subject.Items.First().Items.Count == 1;

    public static IReadOnlyList<T> Get<T>(this GraphQueryResults subject) => subject.NotNull()
        .Items.First()
        .Items.OfType<T>()
        .ToArray();
}