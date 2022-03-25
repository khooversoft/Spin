using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Toolbox.Logging;
using Toolbox.Services;
using Toolbox.Tools;

namespace Toolbox.Azure.Queue
{
    public class QueueClient<T> : IAsyncDisposable where T : class
    {
        private readonly ILogger<QueueClient<T>> _logger;
        protected MessageSender _messageSender;

        public QueueClient(QueueOption queueOption, ILogger<QueueClient<T>> logger)
        {
            queueOption.VerifyNotNull(nameof(queueOption));
            logger.VerifyNotNull(nameof(logger));

            _logger = logger;

            _messageSender = new MessageSender(queueOption.ToConnectionString(), queueOption.QueueName);
        }

        public async Task Close()
        {
            MessageSender messsageSender = Interlocked.Exchange(ref _messageSender, null!);
            if (messsageSender != null)
            {
                await messsageSender.CloseAsync();
            }
        }

        public async ValueTask DisposeAsync() => await Close();

        /// <summary>
        /// Send message, fire and forget
        /// </summary>
        /// <param name="payload">payload for message</param>
        /// <returns></returns>
        public async Task Send(T payload)
        {
            if (_messageSender == null || _messageSender?.IsClosedOrClosing == true) return;

            Message message = payload.ToMessage(Guid.NewGuid().ToString());

            _logger.LogTrace("Sending message: contentType={contentType}, data.Length {length}, MessageId={messageId}", message.ContentType, message.Body?.Length, message.MessageId);

            await _messageSender!.SendAsync(message);
        }
    }
}