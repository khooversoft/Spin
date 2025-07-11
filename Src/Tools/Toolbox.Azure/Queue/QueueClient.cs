using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace Toolbox.Azure.Queue
{
    public class QueueClient<T> : IAsyncDisposable where T : class
    {
        private readonly ILogger<QueueClient<T>> _logger;
        private ServiceBusSender? _messageSender;
        private ServiceBusClient? _serviceBusClient;

        public QueueClient(QueueOption queueOption, ILogger<QueueClient<T>> logger)
        {
            queueOption.NotNull();
            logger.NotNull();

            _logger = logger;

            _serviceBusClient = new ServiceBusClient(queueOption.ToConnectionString());
            _messageSender = _serviceBusClient.CreateSender(queueOption.QueueName);
        }

        public async Task Close()
        {
            ServiceBusSender? sender = Interlocked.Exchange(ref _messageSender, null);
            if (sender != null)
            {
                await sender.CloseAsync();
            }

            ServiceBusClient? client = Interlocked.Exchange(ref _serviceBusClient, null);
            if (client != null)
            {
                await client.DisposeAsync();
            }
        }

        public async ValueTask DisposeAsync() => await Close();

        /// <summary>
        /// Send message, fire and forget
        /// </summary>
        /// <param name="payload">payload for message</param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task Send(T payload, CancellationToken token = default)
        {
            if (_messageSender == null || _messageSender.IsClosed == true) return;

            ServiceBusMessage message = payload.ToMessage();

            _logger.LogDebug("Sending message: contentType={contentType}, MessageId={messageId}", message.ContentType, message.MessageId);
            await _messageSender.SendMessageAsync(message, token);
        }

        /// <summary>
        /// Send messages in a batch
        /// </summary>
        /// <param name="payloads">list of payloads</param>
        /// <param name="token"></param>
        /// <returns></returns>
        /// <exception cref="Exception">message too large or other error</exception>
        public async Task Send(IEnumerable<T> payloads, CancellationToken token = default)
        {
            payloads.NotNull();
            if (_messageSender == null || _messageSender.IsClosed == true) return;

            var messages = payloads
                .Select(x => x.ToMessage())
                .ToList();

            using ServiceBusMessageBatch messageBatch = await _messageSender.CreateMessageBatchAsync();

            var message = messages
                .Select((x, i) => (index: i, state: messageBatch.TryAddMessage(x)))
                .Where(x => !x.state)
                .Select(x => $"Message {x.index} is too large and cannot be sent")
                .Join(", ");

            if (!message.IsEmpty())
            {
                message += ", batch failed";
                _logger.LogError(message);
                throw new Exception(message);
            }

            messages
                .ForEach(x => _logger.LogDebug("Sending message: contentType={contentType}, MessageId={messageId}", x.ContentType, x.MessageId));

            await _messageSender.SendMessagesAsync(messageBatch, token);
        }
    }
}