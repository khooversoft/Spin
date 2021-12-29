using Azure;
using System;

namespace Toolbox.Azure.DataLake.Model
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