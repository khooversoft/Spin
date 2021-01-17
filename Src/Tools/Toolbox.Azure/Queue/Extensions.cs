using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Management;
using System.Linq;
using System.Text;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace Toolbox.Azure.Queue
{
    public static class Extensions
    {
        public static QueueDescription ConvertTo(this QueueDefinition subject)
        {
            subject.VerifyNotNull(nameof(subject));
            subject.QueueName!.VerifyNotEmpty(nameof(subject.QueueName));

            return new QueueDescription(subject.QueueName)
            {
                LockDuration = subject.LockDuration,
                RequiresDuplicateDetection = subject.RequiresDuplicateDetection,
                DuplicateDetectionHistoryTimeWindow = subject.DuplicateDetectionHistoryTimeWindow,
                RequiresSession = subject.RequiresSession,
                DefaultMessageTimeToLive = subject.DefaultMessageTimeToLive,
                AutoDeleteOnIdle = subject.AutoDeleteOnIdle,
                EnableDeadLetteringOnMessageExpiration = subject.EnableDeadLetteringOnMessageExpiration,
                MaxDeliveryCount = subject.MaxDeliveryCount,
                EnablePartitioning = subject.EnablePartitioning,
            };
        }

        public static QueueDefinition ConvertTo(this QueueDescription subject)
        {
            subject.VerifyNotNull(nameof(subject));

            return new QueueDefinition
            {
                QueueName = subject.Path,
                LockDuration = subject.LockDuration,
                RequiresDuplicateDetection = subject.RequiresDuplicateDetection,
                DuplicateDetectionHistoryTimeWindow = subject.DuplicateDetectionHistoryTimeWindow,
                RequiresSession = subject.RequiresSession,
                DefaultMessageTimeToLive = subject.DefaultMessageTimeToLive,
                AutoDeleteOnIdle = subject.AutoDeleteOnIdle,
                EnableDeadLetteringOnMessageExpiration = subject.EnableDeadLetteringOnMessageExpiration,
                MaxDeliveryCount = subject.MaxDeliveryCount,
                EnablePartitioning = subject.EnablePartitioning,
            };
        }

        public static string ToConnectionString(this QueueOption subject)
        {
            return new ServiceBusConnectionStringBuilder
            {
                Endpoint = $"{subject.Namespace}.servicebus.windows.net",
                SasKeyName = subject.KeyName,
                SasKey = subject.AccessKey,
                TransportType = TransportType.Amqp,
            }.ToString();
        }

        public static void Verify(this QueueOption subject)
        {
            subject.VerifyNotNull(nameof(subject));

            subject.Namespace.VerifyNotEmpty(nameof(subject.Namespace));
            subject.QueueName.VerifyNotEmpty(nameof(subject.QueueName));
            subject.KeyName.VerifyNotEmpty(nameof(subject.KeyName));
            subject.AccessKey.VerifyNotEmpty(nameof(subject.AccessKey));
        }

        public static void Verify(this MessagePayload subject)
        {
            subject.VerifyNotNull(nameof(subject));

            subject.ContentType.VerifyNotEmpty(nameof(subject.ContentType));
            subject.MessageId.VerifyNotNull(nameof(subject.MessageId));
            subject.CorrelationId.VerifyNotNull(nameof(subject.CorrelationId));

            subject.Data
                .VerifyNotNull(nameof(subject.Data))
                .VerifyAssert(x => x.Length > 0, "data is empty");
        }

        public static Message ToMessage(this MessagePayload subject)
        {
            subject.Verify();

            string json = Json.Default.Serialize(subject);
            byte[] body = Encoding.UTF8.GetBytes(json);

            return new Message
            {
                ContentType = nameof(MessagePayload),
                Body = body,
                MessageId = subject.MessageId.ToString(),
                CorrelationId = subject.CorrelationId,
            };
        }

        public static MessagePayload ToMessagePayload(this Message subject)
        {
            subject.VerifyNotNull(nameof(subject));
            subject.ContentType.VerifyAssert(x => x == nameof(MessagePayload), "Invalid content type");

            string json = Encoding.UTF8.GetString(subject.Body);
            json.VerifyNotEmpty(nameof(json));

            MessagePayload messagePayload = Json.Default.Deserialize<MessagePayload>(json)!;
            messagePayload.Verify();

            return messagePayload;
        }

        public static Message ToMessage<T>(this T subject, string messageId) where T : class
        {
            return new Message
            {
                ContentType = nameof(T),
                Body = subject.ToBytes(),
                MessageId = messageId,
            };
        }

        public static T? FromMessage<T>(this Message message)
        {
            message.VerifyNotNull(nameof(message));
            message.ContentType.VerifyAssert(x => x == nameof(T), "Invalid content type");

            return message.Body.ToObject<T>();
        }
    }
}