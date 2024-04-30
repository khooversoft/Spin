using Toolbox.Graph;
using Toolbox.Types;

namespace Toolbox.Orleans;

[GenerateSerializer]
public struct GraphEdge_Surrogate
{
    [Id(0)] public Guid Key;
    [Id(1)] public string FromKey;
    [Id(2)] public string ToKey;
    [Id(3)] public string EdgeType;
    [Id(4)] public string Tags;
    [Id(5)] public DateTime CreatedDate;
}


[RegisterConverter]
public sealed class GraphEdge_SurrogateConverter : IConverter<GraphEdge, GraphEdge_Surrogate>
{
    public GraphEdge ConvertFromSurrogate(in GraphEdge_Surrogate surrogate) => new GraphEdge(
        key: surrogate.Key,
        fromKey: surrogate.FromKey,
        toKey: surrogate.ToKey,
        edgeType: surrogate.EdgeType,
        tags: surrogate.Tags.ToTags(),
        createdDate: surrogate.CreatedDate
        );

    public GraphEdge_Surrogate ConvertToSurrogate(in GraphEdge value) => new GraphEdge_Surrogate
    {
        Key = value.Key,
        FromKey = value.FromKey,
        ToKey = value.ToKey,
        EdgeType = value.EdgeType,
        Tags = value.Tags.ToTagsString(),
        CreatedDate = value.CreatedDate,
    };
}

