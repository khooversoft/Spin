using System.Collections.Frozen;
using Toolbox.Graph;

namespace Toolbox.Orleans;

// TODO, try immutable dict and hashset
[GenerateSerializer]
[Alias("Toolbox.Orleans.GraphNode_Surrogate")]
public struct GraphNode_Surrogate
{
    [Id(0)] public string Key;
    [Id(1)] public KeyValuePair<string, string?>[] Tags;
    [Id(2)] public DateTime CreatedDate;
    [Id(3)] public KeyValuePair<string, GraphLink>[] DataMap;
    [Id(4)] public string[] Indexes;
    [Id(5)] public KeyValuePair<string, string?>[] ForeignKeys;
}


[RegisterConverter]
public sealed class GraphNode_SurrogateConverter : IConverter<GraphNode, GraphNode_Surrogate>
{
    public GraphNode ConvertFromSurrogate(in GraphNode_Surrogate surrogate) => new GraphNode(
        surrogate.Key,
        surrogate.Tags.ToDictionary(),
        surrogate.CreatedDate,
        surrogate.DataMap.ToDictionary(x => x.Key, x => x.Value),
        surrogate.Indexes.ToFrozenSet(),
        surrogate.ForeignKeys.ToDictionary()
        );

    public GraphNode_Surrogate ConvertToSurrogate(in GraphNode value) => new GraphNode_Surrogate
    {
        Key = value.Key,
        Tags = [.. value.Tags.Select(x => x)],
        CreatedDate = value.CreatedDate,
        DataMap = [.. value.DataMap],
        Indexes = [.. value.Indexes],
        ForeignKeys = [.. value.ForeignKeys],
    };
}
