using System;

namespace Toolbox.Azure.DataLake.Model
{
    public record DataLakePathItem
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