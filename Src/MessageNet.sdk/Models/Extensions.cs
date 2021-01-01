using Microsoft.Azure.ServiceBus;
using Toolbox.Azure.Queue;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace MessageNet.sdk.Models
{
    public static class Extensions
    {
        public static void Verify(this MessageOption subject)
        {
            subject.VerifyNotNull(nameof(subject));

            subject.Nodes
                .VerifyNotNull(nameof(subject.Nodes))
                .VerifyAssert(x => x.Count > 0, "Nodes are empty");

            subject.Nodes.ForEach(x => x.Verify());
        }

        public static void Verify(this MessageNodeOption subject)
        {
            subject.VerifyNotNull(nameof(subject));

            subject.EndpointId.VerifyNotNull(nameof(subject.EndpointId));
            subject.BusQueue.Verify();
        }
    }
}