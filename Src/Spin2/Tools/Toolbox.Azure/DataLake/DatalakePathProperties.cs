using Azure;
using Azure.Storage.Files.DataLake.Models;
using Toolbox.Azure.DataLake;
using Toolbox.Tools;

namespace Toolbox.Azure.DataLake
{
    public record DatalakePathProperties
    {
        public string Path { get; init; } = null!;
        public DateTimeOffset LastModified { get; init; }
        public string ContentEncoding { get; init; } = null!;
        public ETag ETag { get; init; }
        public string ContentType { get; init; } = null!;
        public long ContentLength { get; init; }
        public DateTimeOffset CreatedOn { get; init; }
    }
}


public static class DatalakePathPropertiesExtensions
{
    public static DatalakePathProperties ConvertTo(this PathProperties subject, string path)
    {
        subject.NotNull();
        path.NotEmpty();

        return new DatalakePathProperties
        {
            Path = path,
            LastModified = subject.LastModified,
            ContentEncoding = subject.ContentEncoding,
            ETag = subject.ETag,
            ContentType = subject.ContentType,
            ContentLength = subject.ContentLength,
            CreatedOn = subject.CreatedOn,
        };
    }
}