namespace Toolbox.Data;

public record DataPipelineOption
{
    public TimeSpan? MemoryCacheDuration { get; set; }
    public TimeSpan? FileCacheDuration { get; set; }
}
