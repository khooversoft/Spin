using System.Collections.Generic;

namespace Toolbox.BlockDocument
{
    public record BlockBlobModel : IDataBlockModelType
    {
        public string? Name { get; init; }

        public string? ContentType { get; init; }

        public string? Author { get; init; }

        public IReadOnlyList<byte>? Content { get; init; }
    }
}
