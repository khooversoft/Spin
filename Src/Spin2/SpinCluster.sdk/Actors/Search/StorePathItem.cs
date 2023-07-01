using Toolbox.Azure.DataLake;
using Toolbox.Tools;

namespace SpinCluster.sdk.Actors.Search;

[GenerateSerializer, Immutable]
public record StorePathItem
{
    [Id(0)] public string Name { get; init; } = null!;
    [Id(1)] public bool? IsDirectory { get; init; }
    [Id(2)] public DateTimeOffset LastModified { get; init; }
    [Id(3)] public string ETag { get; init; } = null!;
    [Id(4)] public long? ContentLength { get; init; }
}


public static class StorePathItemExtensions
{
    public static StorePathItem ConvertTo(this DatalakePathItem subject)
    {
        subject.NotNull();

        return new StorePathItem
        {
            Name = subject.Name,
            IsDirectory = subject.IsDirectory,
            LastModified = subject.LastModified,
            ETag = subject.ETag.ToString(),
            ContentLength = subject.ContentLength,
        };
    }
}