namespace Toolbox.Models;

public record StorePathItem
{
    public required string Path { get; init; }
    public bool? IsDirectory { get; init; }
    public DateTimeOffset? LastModified { get; init; }
    public string? ETag { get; init; }
    public long? ContentLength { get; init; }
    public string? Owner { get; init; }
    public string? Group { get; init; }
    public string? Permissions { get; init; }
}
