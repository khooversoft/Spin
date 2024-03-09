namespace Toolbox.Graph;

public record GraphNodeDelete : IGraphQL
{
    public IReadOnlyList<IGraphQL> Search { get; init; } = Array.Empty<IGraphQL>();
}
