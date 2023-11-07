using Toolbox.Data;
using Toolbox.Types;

namespace SpinCluster.sdk.Serialization;

[GenerateSerializer]
public struct GraphNode_Surrogate
{
    [Id(0)] public string Key;
    [Id(1)] public string Tags;
    [Id(2)] public DateTime CreatedDate;
}


[RegisterConverter]
public sealed class GraphNode_SurrogateConverter : IConverter<GraphNode, GraphNode_Surrogate>
{
    public GraphNode ConvertFromSurrogate(in GraphNode_Surrogate surrogate) => new GraphNode
    {
        Key = surrogate.Key,
        Tags = new Tags().Set(surrogate.Tags),
        CreatedDate = surrogate.CreatedDate,
    };

    public GraphNode_Surrogate ConvertToSurrogate(in GraphNode value) => new GraphNode_Surrogate
    {
        Key = value.Key,
        Tags = value.Tags.ToString(),
        CreatedDate = value.CreatedDate,
    };
}

