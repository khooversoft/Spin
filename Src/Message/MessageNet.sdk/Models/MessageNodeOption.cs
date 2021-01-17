using MessageNet.sdk.Protocol;
using Toolbox.Azure.Queue;

namespace MessageNet.sdk.Models
{
    public record MessageNodeOption
    {
        public string EndpointId { get; init; } = null!;

        public QueueOption BusQueue { get; init; } = null!;
    }
}
