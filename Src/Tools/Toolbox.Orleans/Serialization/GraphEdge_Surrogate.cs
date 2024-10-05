using System.Collections.Immutable;
using Toolbox.Graph;

namespace Toolbox.Orleans;

[GenerateSerializer]
public struct GraphEdge_Surrogate
{
    [Id(1)] public string FromKey;
    [Id(2)] public string ToKey;
    [Id(3)] public string EdgeType;
    [Id(4)] public KeyValuePair<string, string?>[] Tags;
    [Id(5)] public DateTime CreatedDate;
}


[RegisterConverter]
public sealed class GraphEdge_SurrogateConverter : IConverter<GraphEdge, GraphEdge_Surrogate>
{
    public GraphEdge ConvertFromSurrogate(in GraphEdge_Surrogate surrogate) => new GraphEdge(
        fromKey: surrogate.FromKey,
        toKey: surrogate.ToKey,
        edgeType: surrogate.EdgeType,
        tags: surrogate.Tags.ToImmutableDictionary<string, string?>(),
        createdDate: surrogate.CreatedDate
        );

    public GraphEdge_Surrogate ConvertToSurrogate(in GraphEdge value) => new GraphEdge_Surrogate
    {
        FromKey = value.FromKey,
        ToKey = value.ToKey,
        EdgeType = value.EdgeType,
        Tags = value.Tags.ToArray(),
        CreatedDate = value.CreatedDate,
    };
}

