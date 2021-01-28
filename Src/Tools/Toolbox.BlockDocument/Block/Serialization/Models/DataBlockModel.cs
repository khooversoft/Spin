using System;
using System.Collections.Generic;
using System.Text;

namespace Toolbox.BlockDocument
{
    public class DataBlockModel<T> : IDataBlockModel
        where T : IDataBlockModelType
    {
        public long TimeStamp { get; set; }

        public string? BlockType { get; set; }

        public string? BlockId { get; set; }

        public T Data { get; set; } = default!;

        public IReadOnlyDictionary<string, string>? Properties { get; set; }

        public string? Digest { get; set; }

        public string? JwtSignature { get; set; }
    }
}
