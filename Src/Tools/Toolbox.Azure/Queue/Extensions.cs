using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using System;
using System.Text;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace Toolbox.Azure.Queue
{
    public static class Extensions
    {
        public static CreateQueueOptions ToCreateQueue(this QueueDefinition subject)
        {
            subject.NotNull();
            subject.QueueName!.NotEmpty();

            var option = new CreateQueueOptions(subject.QueueName)
            {
                LockDuration = subject.LockDuration,
                RequiresDuplicateDetection = subject.RequiresDuplicateDetection,
                DuplicateDetectionHistoryTimeWindow = subject.DuplicateDetectionHistoryTimeWindow,
                RequiresSession = subject.RequiresSession,
                DefaultMessageTimeToLive = subject.DefaultMessageTimeToLive,
                //AutoDeleteOnIdle = subject.AutoDeleteOnIdle,
                DeadLetteringOnMessageExpiration = subject.DeadLetteringOnMessageExpiration,
                MaxDeliveryCount = subject.MaxDeliveryCount,
                EnablePartitioning = subject.EnablePartitioning,
            };

            option.AuthorizationRules.Add(new SharedAccessAuthorizationRule(
                "allClaims",
                new[] { AccessRights.Manage, AccessRights.Send, AccessRights.Listen }));

            return option;
        }

        public static QueueDefinition ConvertTo(this CreateQueueOptions subject)
        {
            subject.NotNull();

            return new QueueDefinition
            {
                QueueName = subject.Name,
                LockDuration = subject.LockDuration,
                RequiresDuplicateDetection = subject.RequiresDuplicateDetection,
                DuplicateDetectionHistoryTimeWindow = subject.DuplicateDetectionHistoryTimeWindow,
                RequiresSession = subject.RequiresSession,
                DefaultMessageTimeToLive = subject.DefaultMessageTimeToLive,
                AutoDeleteOnIdle = subject.AutoDeleteOnIdle,
                DeadLetteringOnMessageExpiration = subject.DeadLetteringOnMessageExpiration,
                MaxDeliveryCount = subject.MaxDeliveryCount,
                EnablePartitioning = subject.EnablePartitioning,
            };
        }

        public static QueueDefinition ConvertTo(this QueueProperties subject)
        {
            subject.NotNull();

            return new QueueDefinition
            {
                QueueName = subject.Name,
                LockDuration = subject.LockDuration,
                RequiresDuplicateDetection = subject.RequiresDuplicateDetection,
                DuplicateDetectionHistoryTimeWindow = subject.DuplicateDetectionHistoryTimeWindow,
                RequiresSession = subject.RequiresSession,
                DefaultMessageTimeToLive = subject.DefaultMessageTimeToLive,
                AutoDeleteOnIdle = subject.AutoDeleteOnIdle,
                DeadLetteringOnMessageExpiration = subject.DeadLetteringOnMessageExpiration,
                MaxDeliveryCount = subject.MaxDeliveryCount,
                EnablePartitioning = subject.EnablePartitioning,
            };
        }

        public static string ToConnectionString(this QueueOption subject)
        {
            return $"Endpoint=sb://{subject.Namespace}.servicebus.windows.net/;SharedAccessKeyName={subject.KeyName};SharedAccessKey={subject.AccessKey}";
        }

        public static QueueOption Verify(this QueueOption subject)
        {
            subject.NotNull();

            subject.Namespace.NotEmpty();
            subject.QueueName.NotEmpty();
            subject.KeyName.NotEmpty();
            subject.AccessKey.NotEmpty();

            return subject;
        }

        public static void Verify<T>(this QueueReceiverOption<T> subject) where T : class
        {
            subject.NotNull();

            subject.QueueOption.Verify();
            subject.Receiver.NotNull();
        }

        public static ServiceBusMessage ToMessage<T>(this T subject) where T : class
        {
            return new ServiceBusMessage
            {
                ContentType = nameof(T),
                Body = new BinaryData(subject.ToBytes()),
                MessageId = Guid.NewGuid().ToString(),
            };
        }

        public static T? FromMessage<T>(this ServiceBusMessage message)
        {
            message.NotNull();
            message.ContentType.Assert(x => x == nameof(T), "Invalid content type");

            return message.Body.ToArray().ToObject<T>();
        }
    }
}