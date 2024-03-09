namespace Toolbox.Graph;

public record GraphEdgeDelete : IGraphQL
{
    public IReadOnlyList<IGraphQL> Search { get; init; } = Array.Empty<IGraphQL>();
}

