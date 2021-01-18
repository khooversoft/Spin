using System;
using System.Text;
using Toolbox.Tools;

namespace MessageNet.sdk.Protocol
{
    public record Content
    {
        public Guid ContentId { get; init; } = Guid.NewGuid();

        public string ContentType { get; init; } = null!;

        public string? Data { get; init; }
    }
}
