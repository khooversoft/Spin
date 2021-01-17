using System;

namespace Toolbox.Azure.DataLake.Model
{
    public record DatalakePathProperties
    {
        public DateTimeOffset LastModified { get; init; }

        public string ContentEncoding { get; init; } = null!;

        public string ETag { get; init; } = null!;

        public string ContentType { get; init; } = null!;

        public long ContentLength { get; init; }

        public DateTimeOffset CreatedOn { get; init; }
    }
}