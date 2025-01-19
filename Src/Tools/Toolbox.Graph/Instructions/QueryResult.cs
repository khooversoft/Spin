using System.Collections.Immutable;
using System.Text;
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

public record NodeDataLinkObject<T>
{
    public string NodeKey { get; init; } = null!;
    public string Name { get; init; } = null!;
    public T Data { get; init; } = default!;
}

public static class QueryResultTool
{
    public static QueryResult? Query(this QueryBatchResult subject, int queryNumber) => subject.NotNull().Items.SingleOrDefault(x => x.QueryNumber == queryNumber);
    public static QueryResult? Alias(this QueryBatchResult subject, string alias) => subject.NotNull().Items.SingleOrDefault(x => x.Alias == alias);

    public static IReadOnlyList<NodeDataLinkObject<T>> GetDataLinks<T>(this QueryResult subject, string dataName)
    {
        subject.NotNull();
        dataName.NotEmpty();

        var list = subject.DataLinks
            .Where(x => x.Name.EqualsIgnoreCase(dataName))
            .Select(x => new NodeDataLinkObject<T>
            {
                NodeKey = x.NodeKey,
                Name = x.Name,
                Data = x.Data.ToObject<T>(),
            })
            .ToImmutableArray();

        return list;
    }

    public static IReadOnlyList<T> DataLinkToObjects<T>(this QueryResult subject, string dataName)
    {
        subject.NotNull();
        dataName.NotEmpty();

        var list = subject.DataLinks
            .Where(x => x.Name.EqualsIgnoreCase(dataName))
            .Select(x => x.Data.ToObject<T>())
            .ToImmutableArray();

        return list;
    }

    public static Option<T> DataLinkToObject<T>(this QueryResult subject, string dataName)
    {
        subject.NotNull();
        dataName.NotEmpty();

        if (subject.DataLinks.Count != 1) return (StatusCode.Conflict, "There is more then 1 data link, must specify the node key");

        var list = subject.GetDataLinks<T>(dataName);
        if (list.Count == 0) return StatusCode.NotFound;

        return list[0].Data;
    }

    public static Option<T> DataLinkToObject<T>(this QueryResult subject, string dataName, string nodeKey)
    {
        nodeKey.NotEmpty();

        var item = subject.GetDataLinks<T>(dataName).FirstOrDefault(x => x.NodeKey.EqualsIgnoreCase(nodeKey));
        if (item == null) return StatusCode.NotFound;

        return item.Data;
    }

    public static string DumpToString(this QueryResult subject)
    {
        subject.NotNull();

        (string key, string value)[] list = new []
        {
            ("Status", subject.Option.ToString()),
            ("QueryNumber", subject.QueryNumber.ToString()),
            ("Alias", subject.Alias ?? "< null >"),
        }
        .Concat(subject.Nodes.Select(x => ("Node", x.ToString())))
        .Concat(subject.Edges.Select(x => ("Edges", x.ToString())))
        .Concat(subject.DataLinks.Select(x => ("DataLink", x.ToString())))
        .ToArray();

        string result = list
            .Select(x => $"{x.key}={x.value}")
            .Join(Environment.NewLine);

        return result;
    }
}
