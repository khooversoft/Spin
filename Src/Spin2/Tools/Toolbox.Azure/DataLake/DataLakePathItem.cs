using Azure.Storage.Files.DataLake.Models;
using Toolbox.Azure.DataLake;
using Toolbox.Tools;

namespace Toolbox.Azure.DataLake
{
    public record DatalakePathItem
    {
        public string Name { get; init; } = null!;
        public bool? IsDirectory { get; init; }
        public DateTimeOffset LastModified { get; init; }
        public string ETag { get; init; } = null!;
        public long? ContentLength { get; init; }
        public string? Owner { get; init; }
        public string? Group { get; init; }
        public string? Permissions { get; init; }
    }
}


public static class DatalakePathItemExtensions
{
    public static DatalakePathItem ConvertTo(this PathItem subject)
    {
        subject.NotNull();

        return new DatalakePathItem
        {
            Name = subject.Name,
            IsDirectory = subject.IsDirectory,
            LastModified = subject.LastModified,
            ETag = subject.ETag.ToString(),
            ContentLength = subject.ContentLength,
            Owner = subject.Owner,
            Group = subject.Group,
            Permissions = subject.Permissions,
        };
    }
}