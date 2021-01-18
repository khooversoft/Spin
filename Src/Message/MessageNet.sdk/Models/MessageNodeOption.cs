using MessageNet.sdk.Protocol;
using Toolbox.Azure.Queue;

namespace MessageNet.sdk.Models
{
    public record MessageNodeOption
    {
        public string EndpointId { get; init; } = null!;

        public QueueOption BusQueue { get; init; } = null!;

        public bool AutoComplete { get; init; } = false;

        public int MaxConcurrentCalls { get; init; } = 10;
    }
}
