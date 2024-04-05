using System.Collections.Immutable;
using Toolbox.Graph;
using Toolbox.Types;

namespace SpinCluster.sdk.Serialization;

[GenerateSerializer]
public struct GraphNode_Surrogate
{
    [Id(0)] public string Key;
    [Id(1)] public string Tags;
    [Id(2)] public DateTime CreatedDate;
    [Id(3)] public string[] Links;
}


[RegisterConverter]
public sealed class GraphNode_SurrogateConverter : IConverter<GraphNode, GraphNode_Surrogate>
{
    public GraphNode ConvertFromSurrogate(in GraphNode_Surrogate surrogate) => new GraphNode(
        surrogate.Key,
        new Tags().Set(surrogate.Tags),
        surrogate.CreatedDate,
        surrogate.Links.ToImmutableArray()
        );

    public GraphNode_Surrogate ConvertToSurrogate(in GraphNode value) => new GraphNode_Surrogate
    {
        Key = value.Key,
        Tags = value.Tags.ToString(),
        CreatedDate = value.CreatedDate,
        Links = value.Links.ToArray(),
    };
}

