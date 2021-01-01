using System.Collections.Generic;
using System.Linq;

namespace MessageNet.sdk.Protocol
{
    public record Packet
    {
        public string Version { get; init; } = "1.0";

        public IReadOnlyList<Message> Messages { get; init; } = null!;

        public Message Message => (Messages ?? new[] { new Message() }).First();
    }
}