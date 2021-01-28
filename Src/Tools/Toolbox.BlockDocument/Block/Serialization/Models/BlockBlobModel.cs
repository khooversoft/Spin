using System.Collections.Generic;

namespace Toolbox.BlockDocument
{
    public class BlockBlobModel : IDataBlockModelType
    {
        public string? Name { get; set; }

        public string? ContentType { get; set; }

        public string? Author { get; set; }

        public IReadOnlyList<byte>? Content { get; set; }
    }
}
