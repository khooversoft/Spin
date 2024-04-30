using Toolbox.Graph;
using Toolbox.Types;

namespace Toolbox.Orleans;

// TODO, try immutable dict and hashset
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
        surrogate.Tags.ToTags(),
        surrogate.CreatedDate,
        surrogate.Links.ToLinks()
        );

    public GraphNode_Surrogate ConvertToSurrogate(in GraphNode value) => new GraphNode_Surrogate
    {
        Key = value.Key,
        Tags = value.Tags.ToTagsString(),
        CreatedDate = value.CreatedDate,
        Links = value.Links.ToArray(),
    };
}

