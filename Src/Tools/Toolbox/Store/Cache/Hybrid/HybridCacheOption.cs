namespace Toolbox.Store;

public record HybridCacheOption
{
    public TimeSpan MemoryCacheDuration { get; init; } = TimeSpan.FromMinutes(5);
    public TimeSpan FileCacheDuration { get; init; } = TimeSpan.FromDays(5);
}
