namespace Toolbox.Data;

public record GraphNodeAdd : IGraphQL
{
    public string Key { get; init; } = null!;
    public string? Tags { get; init; }
}
