namespace Toolbox.Data;

public record GraphNodeUpdate : IGraphQL
{
    public string? Tags { get; init; }
    public IReadOnlyList<IGraphQL> Search { get; init; } = Array.Empty<IGraphQL>();
}
