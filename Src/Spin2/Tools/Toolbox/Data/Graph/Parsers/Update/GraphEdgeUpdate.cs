namespace Toolbox.Data;

public record GraphEdgeUpdate : IGraphQL
{
    public string FromKey { get; init; } = null!;
    public string ToKey { get; init; } = null!;
    public string? EdgeType { get; init; }
    public string? Tags { get; init; }
    public EdgeDirection Direction { get; init; } = EdgeDirection.Both;
}
