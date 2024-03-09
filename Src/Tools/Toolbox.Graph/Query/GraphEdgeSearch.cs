using Toolbox.Extensions;
using Toolbox.Types;

namespace Toolbox.Graph;

public record GraphEdgeSearch : IGraphQL
{
    public string? NodeKey { get; init; }
    public string? FromKey { get; init; }
    public string? ToKey { get; init; }
    public string? EdgeType { get; init; }
    public Tags Tags { get; init; } = new Tags();
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
            string => edge.FromKey.IsMatch(subject.NodeKey) || edge.ToKey.IsMatch(subject.NodeKey),
        };

        bool isFromKey = subject.FromKey == null || edge.FromKey.IsMatch(subject.FromKey);
        bool isToKey = subject.ToKey == null || edge.ToKey.IsMatch(subject.ToKey);
        bool isEdgeType = subject.EdgeType == null || edge.EdgeType.IsMatch(subject.EdgeType);
        bool isTag = subject.Tags.Count == 0 || edge.Tags.Has(subject.Tags);

        return isNodeKey && isFromKey && isToKey && isEdgeType && isTag;
    }
}