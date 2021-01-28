using System;
using System.Collections.Generic;
using System.Text;

namespace Toolbox.BlockDocument
{
    public record DataBlockModel<T> : IDataBlockModel
        where T : IDataBlockModelType
    {
        public long TimeStamp { get; init; }

        public string? BlockType { get; init; }

        public string? BlockId { get; init; }

        public T Data { get; init; } = default!;

        public IReadOnlyDictionary<string, string>? Properties { get; init; }

        public string? Digest { get; init; }

        public string? JwtSignature { get; init; }
    }
}
