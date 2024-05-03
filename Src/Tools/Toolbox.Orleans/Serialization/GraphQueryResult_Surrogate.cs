using System.Collections.Immutable;
using Toolbox.Extensions;
using Toolbox.Graph;
using Toolbox.Types;

namespace Toolbox.Orleans;

[GenerateSerializer]
public struct GraphQueryResult_Surrogate
{
    [Id(0)] public Option Status;
    [Id(1)] public CommandType? CommandType;
    [Id(2)] public IReadOnlyList<GraphNode> Nodes;
    [Id(3)] public IReadOnlyList<GraphEdge> Edges;
    [Id(4)] public IReadOnlyDictionary<string, IReadOnlyList<GraphNode>> NodeAlias;
    [Id(5)] public IReadOnlyDictionary<string, IReadOnlyList<GraphEdge>> EdgeAlias;
}


[RegisterConverter]
public sealed class GraphQueryResult_SurrogateConverter : IConverter<GraphQueryResult, GraphQueryResult_Surrogate>
{
    public GraphQueryResult ConvertFromSurrogate(in GraphQueryResult_Surrogate surrogate)
    {
        Dictionary<string, IReadOnlyList<IGraphCommon>> alias = surrogate.NodeAlias
            .Select(x => new KeyValuePair<string, IReadOnlyList<IGraphCommon>>(x.Key, x.Value.OfType<IGraphCommon>().ToArray()))
            .Concat(surrogate.EdgeAlias.Select(x => new KeyValuePair<string, IReadOnlyList<IGraphCommon>>(x.Key, x.Value.OfType<IGraphCommon>().ToArray())))
            .ToDictionary(x => x.Key, x => x.Value, StringComparer.OrdinalIgnoreCase);

        var result = new GraphQueryResult
        {
            Status = surrogate.Status,
            CommandType = surrogate.CommandType,
            Items = surrogate.Nodes.OfType<IGraphCommon>().Concat(surrogate.Edges).ToImmutableArray(),
            Alias = alias.ToImmutableDictionary(x => x.Key, x => x.Value.ToImmutableArray()),
        };

        return result;
    }

    public GraphQueryResult_Surrogate ConvertToSurrogate(in GraphQueryResult subject)
    {
        var nodeAlias = subject.Alias
            .Select(x => new KeyValuePair<string, IReadOnlyList<GraphNode>>(x.Key, x.Value.OfType<GraphNode>().ToArray()))
            .ToDictionary(x => x.Key, x => x.Value, StringComparer.OrdinalIgnoreCase);

        var edgeAlias = subject.Alias
            .Select(x => new KeyValuePair<string, IReadOnlyList<GraphEdge>>(x.Key, x.Value.OfType<GraphEdge>().ToArray()))
            .ToDictionary(x => x.Key, x => x.Value, StringComparer.OrdinalIgnoreCase);

        var result = new GraphQueryResult_Surrogate
        {
            Status = subject.Status,
            CommandType = subject.CommandType,
            Nodes = subject.Items.OfType<GraphNode>().ToArray(),
            Edges = subject.Items.OfType<GraphEdge>().ToArray(),
            NodeAlias = nodeAlias,
            EdgeAlias = edgeAlias,
        };

        return result;
    }
}