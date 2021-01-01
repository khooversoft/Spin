using MessageNet.sdk.Protocol;
using Toolbox.Azure.Queue;

namespace MessageNet.sdk.Models
{
    public record MessageNodeOption
    {
        public EndpointId EndpointId { get; init; } = null!;

        public QueueOption BusQueue { get; init; } = null!;
    }
}
