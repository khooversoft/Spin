using MessageNet.sdk.Protocol;
using Microsoft.Azure.ServiceBus;
using System.Linq;
using Toolbox.Azure.Queue;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace MessageNet.sdk.Models
{
    public static class Extensions
    {
        public static void Verify(this MessageHostOption subject)
        {
            subject.VerifyNotNull(nameof(subject));

            subject.Nodes
                .VerifyNotNull(nameof(subject.Nodes))
                .VerifyAssert(x => x.Count > 0, "Nodes are empty");

            subject.Nodes.ForEach(x => x.Verify());

            subject.Nodes
                .GroupBy(x => (string)x.EndpointId)
                .Where(x => x.Count() > 1)
                .VerifyAssert(x => !x.Any(), x => $"Duplicate endpoint(s): {string.Join(", ", x)}");
        }

        public static void Verify(this MessageNodeOption subject)
        {
            subject.VerifyNotNull(nameof(subject));

            subject.EndpointId.VerifyNotNull(nameof(subject.EndpointId));
            subject.BusQueue.Verify();
        }

        public static void Verify(this BusNamespaceOption subject)
        {
            subject.VerifyNotNull(nameof(subject));

            subject.Namespace.VerifyNotEmpty(nameof(subject.Namespace));
            subject.BusNamespace.VerifyNotEmpty(nameof(subject.BusNamespace));
            subject.KeyName.VerifyNotEmpty(nameof(subject.KeyName));
            subject.AccessKey.VerifyNotEmpty(nameof(subject.AccessKey));
        }
    }
}