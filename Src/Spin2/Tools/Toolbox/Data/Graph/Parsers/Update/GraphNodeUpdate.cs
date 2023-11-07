namespace Toolbox.Data;

public record GraphNodeUpdate : IGraphQL
{
    public string Key { get; init; } = null!;
    public string? Tags { get; init; }
}
