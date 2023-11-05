using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Data;

public record GraphQueryResult
{
    public StatusCode StatusCode { get; init; }
    public string? Error { get; init; }

    public IReadOnlyList<IGraphCommon> Items { get; init; } = null!;
    public IReadOnlyDictionary<string, IReadOnlyList<IGraphCommon>> Alias { get; init; } = null!;
}


public static class GraphQueryResultExtensions
{
    public static IReadOnlyList<GraphEdge> Edges(this GraphQueryResult subject) => subject.NotNull().Items.OfType<GraphEdge>().ToArray();
    public static IReadOnlyList<GraphNode> Nodes(this GraphQueryResult subject) => subject.NotNull().Items.OfType<GraphNode>().ToArray();

    public static IReadOnlyList<GraphEdge> AliasEdge(this GraphQueryResult subject, string key) => subject.NotNull().Alias[key].OfType<GraphEdge>().ToArray();
    public static IReadOnlyList<GraphNode> AliasNode(this GraphQueryResult subject, string key) => subject.NotNull().Alias[key].OfType<GraphNode>().ToArray();
}