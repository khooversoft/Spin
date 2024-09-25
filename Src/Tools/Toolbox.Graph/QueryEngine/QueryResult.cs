using Toolbox.Extensions;
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
    public IReadOnlyList<GraphLinkData> DataLinks { get; init; } = Array.Empty<GraphLinkData>();
}

public static class QueryResultTool
{
    public static QueryResult? Query(this QueryBatchResult subject, int queryNumber) => subject.NotNull().Items.SingleOrDefault(x => x.QueryNumber == queryNumber);
    public static QueryResult? Alias(this QueryBatchResult subject, string alias) => subject.NotNull().Items.SingleOrDefault(x => x.Alias == alias);

    public static Option<T> DataLinkToObject<T>(this QueryResult subject, string dataName)
    {
        subject.NotNull();
        dataName.NotEmpty();

        var dataLink = subject.DataLinks.SingleOrDefault(x => x.Name.EqualsIgnoreCase(dataName));
        if (dataLink == null) return StatusCode.NotFound;

        return dataLink.Data.ToObject<T>();
    }
}