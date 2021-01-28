using System;

namespace Toolbox.BlockDocument
{
    public class HeaderBlockModel : IDataBlockModelType
    {
        public long TimeStamp { get; set; }

        public string? Description { get; set; }
    }
}
