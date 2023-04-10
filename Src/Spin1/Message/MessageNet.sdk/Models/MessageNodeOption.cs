using MessageNet.sdk.Protocol;
using Toolbox.Azure.Queue;
using Toolbox.Tools;

namespace MessageNet.sdk.Models
{
    public record MessageNodeOption
    {
        public string ServiceId { get; init; } = null!;

        public QueueOption BusQueue { get; init; } = null!;

        public bool AutoComplete { get; init; } = false;

        public int MaxConcurrentCalls { get; init; } = 10;
    }


    public static class MessageNodeOptionExtensions
    {        public static void Verify(this MessageNodeOption subject)
        {
            subject.VerifyNotNull(nameof(subject));

            subject.ServiceId.VerifyNotNull(nameof(subject.ServiceId));
            subject.BusQueue.Verify();
        }
    }
}
