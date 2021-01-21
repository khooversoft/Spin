using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using Toolbox.Services;
using Toolbox.Tools;

namespace Toolbox.Azure.Queue
{
    public class QueueClient<T> : IAsyncDisposable where T : class
    {
        private readonly Func<T, Guid?> _getId;
        private readonly IAwaiterCollection<T> _awaiterService;
        private readonly ILogger<QueueClient<T>> _logger;
        private MessageSender _messageSender;

        public QueueClient(Func<T, Guid?> getId, QueueOption queueOption, IAwaiterCollection<T> awaiterService, ILogger<QueueClient<T>> logger)
        {
            getId.VerifyNotNull(nameof(getId));
            queueOption.VerifyNotNull(nameof(queueOption));
            awaiterService.VerifyNotNull(nameof(awaiterService));
            logger.VerifyNotNull(nameof(logger));

            _getId = getId;
            _awaiterService = awaiterService;
            _logger = logger;

            _messageSender = new MessageSender(queueOption.ToConnectionString(), queueOption.QueueName);
        }

        /// <summary>
        /// Call endpoint, expect a single return message
        /// </summary>
        /// <param name="payload">payload for message</param>
        /// <param name="timeout">timeout, null for default</param>
        /// <returns></returns>
        public async Task<T> Call(T payload, TimeSpan? timeout = null)
        {
            Message message = payload.ToMessage(Guid.NewGuid().ToString());

            _logger.LogTrace($"Calling message: contentType={message.ContentType}, data.Length {message.Body?.Length}");

            await _messageSender!.SendAsync(message);

            return await WaitForResponse(_getId(payload), timeout);
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

            _logger.LogTrace($"Sending message: contentType={message.ContentType}, data.Length {message.Body?.Length}");

            await _messageSender!.SendAsync(message);
        }

        private Task<T> WaitForResponse(Guid? messageId, TimeSpan? timeout = null)
        {
            if (messageId == null) return Task.FromResult<T>(default!);

            var tcs = new TaskCompletionSource<T>();
            _awaiterService.Register((Guid)messageId, tcs, timeout);

            return tcs.Task;
        }
    }
}