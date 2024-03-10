using Toolbox.Graph;
using Toolbox.Types;

namespace SpinCluster.sdk.Serialization;

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
    public GraphEdge ConvertFromSurrogate(in GraphEdge_Surrogate surrogate) => new GraphEdge
    {
        Key = surrogate.Key,
        FromKey = surrogate.FromKey,
        ToKey = surrogate.ToKey,
        EdgeType = surrogate.EdgeType,
        Tags = new Tags().Set(surrogate.Tags),
        CreatedDate = surrogate.CreatedDate,
    };

    public GraphEdge_Surrogate ConvertToSurrogate(in GraphEdge value) => new GraphEdge_Surrogate
    {
        Key = value.Key,
        FromKey = value.FromKey,
        ToKey = value.ToKey,
        EdgeType = value.EdgeType,
        Tags = value.Tags.ToString(),
        CreatedDate = value.CreatedDate,
    };
}

