namespace Toolbox.Data;

public record GraphNodeQuery<TKey> where TKey : notnull
{
    public TKey? Key { get; init; }
    public string? MatchTags { get; init; }
}
