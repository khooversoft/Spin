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
    [Id(2)] public ImmutableArray<GraphNode> Nodes;
    [Id(3)] public ImmutableArray<GraphEdge> Edges;
    [Id(4)] public ImmutableDictionary<string, ImmutableArray<GraphNode>> NodeAlias;
    [Id(5)] public ImmutableDictionary<string, ImmutableArray<GraphEdge>> EdgeAlias;
    [Id(5)] public ImmutableDictionary<string, DataETag> ReturnNames;
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
            ReturnNames = surrogate.ReturnNames,
        };

        return result;
    }

    public GraphQueryResult_Surrogate ConvertToSurrogate(in GraphQueryResult subject)
    {
        var nodeAlias = subject.Alias
            .Select(x => new KeyValuePair<string, ImmutableArray<GraphNode>>(x.Key, x.Value.OfType<GraphNode>().ToImmutableArray()))
            .ToImmutableDictionary(x => x.Key, x => x.Value, StringComparer.OrdinalIgnoreCase);

        var edgeAlias = subject.Alias
            .Select(x => new KeyValuePair<string, ImmutableArray<GraphEdge>>(x.Key, x.Value.OfType<GraphEdge>().ToImmutableArray()))
            .ToImmutableDictionary(x => x.Key, x => x.Value, StringComparer.OrdinalIgnoreCase);

        var result = new GraphQueryResult_Surrogate
        {
            Status = subject.Status,
            CommandType = subject.CommandType,
            Nodes = subject.Items.OfType<GraphNode>().ToImmutableArray(),
            Edges = subject.Items.OfType<GraphEdge>().ToImmutableArray(),
            NodeAlias = nodeAlias,
            EdgeAlias = edgeAlias,
            ReturnNames = subject.ReturnNames,
        };

        return result;
    }
}