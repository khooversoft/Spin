using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Tools;

namespace Spin.Common.Configuration.Model
{
    public static class Extensions
    {
        public static void Verify(this QueueModel subject)
        {
            subject.VerifyNotNull(nameof(subject));

            subject.Channel.VerifyNotEmpty($"{nameof(subject.Channel)} is required");
            subject.ServiceBus.Verify();
        }

        public static void Verify(this StorageModel subject)
        {
            subject.VerifyNotNull(nameof(subject));

            subject.Channel.VerifyNotEmpty($"{nameof(subject.AccountName)} is required");
            subject.AccountName.VerifyNotEmpty($"{nameof(subject.AccountName)} is required");
            subject.ContainerName.VerifyNotEmpty($"{nameof(subject.ContainerName)} is required");
            subject.AccountKey.VerifyNotEmpty($"{nameof(subject.AccountName)} is required");
        }

        public static void Verify(this ServiceBusQueue subject)
        {
            subject.VerifyNotNull(nameof(subject));

            subject.Namespace.VerifyNotEmpty($"{nameof(subject.Namespace)} is required");
            subject.Name.VerifyNotEmpty($"{nameof(subject.Name)} is required");
            subject.AuthSendListen.VerifyNotEmpty($"{nameof(subject.AuthSendListen)} is required");
        }
    }
}
