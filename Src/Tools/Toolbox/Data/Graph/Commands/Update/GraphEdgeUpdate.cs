namespace Toolbox.Data;

public record GraphEdgeUpdate : IGraphQL
{
    public string? EdgeType { get; init; }
    public string? Tags { get; init; }
    public IReadOnlyList<IGraphQL> Search { get; init; } = Array.Empty<IGraphQL>();
}
