using Toolbox.Types;

namespace Toolbox.Graph;

public record GraphEdgeUpdate : IGraphQL
{
    public string? EdgeType { get; init; }
    public Tags Tags { get; init; } = new Tags();
    public IReadOnlyList<IGraphQL> Search { get; init; } = Array.Empty<IGraphQL>();
}
