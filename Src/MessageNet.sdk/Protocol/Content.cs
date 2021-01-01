using System;
using System.Text;
using Toolbox.Tools;

namespace MessageNet.sdk.Protocol
{
    public record Content
    {
        public Guid ContentId { get; init; } = Guid.NewGuid();

        public string ContentType { get; init; } = null!;

        public byte[]? Data { get; init; }

        public string DataToString() => Encoding.UTF8.GetString(Data ?? Array.Empty<byte>());
    }
}
