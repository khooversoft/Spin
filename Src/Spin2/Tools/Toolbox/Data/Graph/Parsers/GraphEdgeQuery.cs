namespace Toolbox.Data;

public record GraphEdgeQuery<TKey> : IGraphQL where TKey : notnull
{
    public GraphEdgeQuery() { }

    public TKey? NodeKey { get; init; }
    public TKey? FromKey { get; init; }
    public TKey? ToKey { get; init; }
    public string? EdgeType { get; init; }
    public string? Tags { get; init; }
    public string? Alias { get; init; }
    public EdgeDirection Direction { get; init; } = EdgeDirection.Both;
}

public static class GraphEdgeQueryExtensions
{
    public static bool IsMatch(this GraphEdgeQuery<string> edge)
    {
        bool isNodeKey = NodeKey == null || Key.IsMatch(edge.Key);
        bool isFromKey = FromKey == null || FromKey.IsMatch(edge.FromKey);
        bool isToKey = ToKey == null || ToKey.IsMatch(edge.ToKey);
        bool isEdgeType = EdgeType == null || EdgeType.IsMatch(edge.Key);
        bool isTag = Tags == null || edge.Tags.Has(Tags);
    }
}