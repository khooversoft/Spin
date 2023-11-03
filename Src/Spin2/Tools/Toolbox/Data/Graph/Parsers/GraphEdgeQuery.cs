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
