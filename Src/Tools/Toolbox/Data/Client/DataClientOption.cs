namespace Toolbox.Data;

public record DataClientOption
{
    public TimeSpan MemoryCacheDuration { get; set; } = TimeSpan.FromMinutes(5);
    public TimeSpan FileCacheDuration { get; set; } = TimeSpan.FromDays(5);
}
