namespace Toolbox.Data;

public record GraphNodeQuery<TKey> : IGraphQL where TKey : notnull
{
    public TKey? Key { get; init; }
    public string? Tags { get; init; }
}
