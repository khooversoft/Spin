using System.Collections.Generic;
using System.Linq;

namespace MessageNet.sdk.Protocol
{
    public record MessagePacket
    {
        public IReadOnlyList<Message> Messages { get; init; } = null!;

        public string Version { get; init; } = "1.0";
    }
}