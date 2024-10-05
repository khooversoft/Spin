using System.Collections.Immutable;
using Toolbox.Graph;
using Toolbox.Types;

namespace Toolbox.Orleans.Serialization;

[GenerateSerializer]
public struct QueryResult_Surrogate
{
    [Id(0)] public Option Option { get; init; }
    [Id(1)] public int QueryNumber;
    [Id(2)] public string? Alias;
    [Id(3)] public GraphNode[] Nodes;
    [Id(4)] public GraphEdge[] Edges;
    [Id(5)] public GraphLinkData[] DataLinks;
}


[RegisterConverter]
public sealed class QueryResult_SurrogateConverter : IConverter<QueryResult, QueryResult_Surrogate>
{
    public QueryResult ConvertFromSurrogate(in QueryResult_Surrogate surrogate) => new QueryResult
    {
        Option = surrogate.Option,
        QueryNumber = surrogate.QueryNumber,
        Alias = surrogate.Alias,
        Nodes = surrogate.Nodes.ToImmutableArray(),
        Edges = surrogate.Edges.ToImmutableArray(),
        DataLinks = surrogate.DataLinks.ToImmutableArray(),
    };

    public QueryResult_Surrogate ConvertToSurrogate(in QueryResult value) => new QueryResult_Surrogate
    {
        Option = value.Option,
        QueryNumber = value.QueryNumber,
        Alias = value.Alias,
        Nodes = value.Nodes.ToArray(),
        Edges = value.Edges.ToArray(),
        DataLinks = value.DataLinks.ToArray(),
    };
}
