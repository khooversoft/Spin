namespace NBlog.sdk;

public record ArticleIndex
{
    public required string IndexName { get; init; } = null!;
    public required string ArticleId { get; init; } = null!;
    public required string OrderBy { get; init; }
}
