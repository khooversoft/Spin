using System.Collections.Immutable;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

public record QueryBatchResult
{
    public string TransactionId { get; init; } = Guid.NewGuid().ToString();
    public Option Option { get; init; }
    public IReadOnlyList<QueryResult> Items { get; init; } = Array.Empty<QueryResult>();
}

public record QueryResult
{
    public Option Option { get; init; }
    public int QueryNumber { get; init; }
    public string? Alias { get; init; }
    public IReadOnlyList<GraphNode> Nodes { get; init; } = Array.Empty<GraphNode>();
    public IReadOnlyList<GraphEdge> Edges { get; init; } = Array.Empty<GraphEdge>();
    public IReadOnlyList<GraphLinkData> Data { get; init; } = Array.Empty<GraphLinkData>();
}

public static class QueryResultTool
{
    public static QueryResult? Query(this QueryBatchResult subject, int queryNumber) => subject.NotNull().Items.SingleOrDefault(x => x.QueryNumber == queryNumber);
    public static QueryResult? Alias(this QueryBatchResult subject, string alias) => subject.NotNull().Items.SingleOrDefault(x => x.Alias == alias);
    public static IReadOnlyList<GraphNode> Nodes(this QueryBatchResult subject) => subject.NotNull().Items.Take(1).SelectMany(x => x.Nodes).ToImmutableArray();
    public static IReadOnlyList<GraphEdge> Edges(this QueryBatchResult subject) => subject.NotNull().Items.Take(1).SelectMany(x => x.Edges).ToImmutableArray();
}