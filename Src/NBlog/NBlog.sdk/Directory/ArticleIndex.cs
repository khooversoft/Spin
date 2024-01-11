namespace NBlog.sdk;

public record ArticleIndex
{
    public required string IndexName { get; init; } = null!;
    public required string ArticleId { get; init; } = null!;
    public required string Title { get; init; }
    public required DateTime CreatedDate { get; init; }
}
