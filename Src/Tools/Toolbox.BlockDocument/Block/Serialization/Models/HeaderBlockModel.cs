using System;

namespace Toolbox.BlockDocument
{
    public record HeaderBlockModel : IDataBlockModelType
    {
        public long TimeStamp { get; init; }

        public string? Description { get; init; }
    }
}
