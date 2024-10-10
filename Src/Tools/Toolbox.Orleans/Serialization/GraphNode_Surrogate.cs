using System.Collections.Frozen;
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
    [Id(3)] public KeyValuePair<string, GraphLink>[] DataMap;
    [Id(4)] public string[] Indexes;
}


[RegisterConverter]
public sealed class GraphNode_SurrogateConverter : IConverter<GraphNode, GraphNode_Surrogate>
{
    public GraphNode ConvertFromSurrogate(in GraphNode_Surrogate surrogate) => new GraphNode(
        surrogate.Key,
        surrogate.Tags.ToTags(),
        surrogate.CreatedDate,
        surrogate.DataMap.ToDictionary(x => x.Key, x => x.Value),
        surrogate.Indexes.ToFrozenSet()
        );

    public GraphNode_Surrogate ConvertToSurrogate(in GraphNode value) => new GraphNode_Surrogate
    {
        Key = value.Key,
        Tags = value.Tags.ToTagsString(),
        CreatedDate = value.CreatedDate,
        DataMap = value.DataMap.ToArray(),
        Indexes = value.Indexes.ToArray(),
    };
}

