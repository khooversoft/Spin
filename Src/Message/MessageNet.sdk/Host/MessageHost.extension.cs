using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Directory.sdk.Model;
using Toolbox.Azure.Queue;

namespace MessageNet.sdk.Host
{
    public static class MessageHostExtension
    {
        public static QueueOption ConvertTo(this QueueRecord subject)
        {
            (string KeyName, string AccessKey) = QueueAuthorization.Parse(subject.AuthSendListen);

            return new QueueOption
            {
                QueueName = subject.QueueName,
                Namespace = subject.Namespace,
                KeyName = KeyName,
                AccessKey = AccessKey,
            };
        }
    }
}
