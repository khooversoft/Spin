using System.Collections.Immutable;
using Toolbox.Extensions;
using Toolbox.Types;

namespace Toolbox.Graph;

public record GraphEdgeSearch : IGraphQL
{
    // This will match to FromKey or ToKey
    public string? NodeKey { get; init; }

    public string? FromKey { get; init; }
    public string? ToKey { get; init; }
    public string? EdgeType { get; init; }
    public ImmutableDictionary<string, string?> Tags { get; init; } = ImmutableDictionary<string, string?>.Empty;
    public string? Alias { get; init; }
    public EdgeDirection Direction { get; init; } = EdgeDirection.Both;
}

public static class GraphEdgeQueryExtensions
{
    public static bool IsMatch(this GraphEdgeSearch subject, GraphEdge edge)
    {
        bool isNodeKey = subject.NodeKey switch
        {
            null => true,
            string => edge.FromKey.Like(subject.NodeKey) || edge.ToKey.Like(subject.NodeKey),
        };

        bool isFromKey = subject.FromKey == null || edge.FromKey.Like(subject.FromKey);
        bool isToKey = subject.ToKey == null || edge.ToKey.Like(subject.ToKey);
        bool isEdgeType = subject.EdgeType == null || edge.EdgeType.Like(subject.EdgeType);
        bool isTag = subject.Tags.Count == 0 || edge.Tags.Has(subject.Tags);

        return isNodeKey && isFromKey && isToKey && isEdgeType && isTag;
    }
}