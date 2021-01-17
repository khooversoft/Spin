//using Microsoft.Azure.ServiceBus;
//using Microsoft.Azure.ServiceBus.Core;
//using Microsoft.Extensions.Logging;
//using System;
//using System.Threading;
//using System.Threading.Tasks;
//using Toolbox.Tools;

//namespace Toolbox.Azure.Queue
//{
//    public class QueueClient : IAsyncDisposable
//    {
//        private readonly IQueueAwaiterService _awaiterService;
//        private readonly ILogger<QueueClient> _logger;
//        private MessageSender _messageSender;

//        public QueueClient(QueueOption queueOption, IQueueAwaiterService awaiterService, ILogger<QueueClient> logger)
//        {
//            queueOption.VerifyNotNull(nameof(queueOption));
//            awaiterService.VerifyNotNull(nameof(awaiterService));
//            logger.VerifyNotNull(nameof(logger));

//            _awaiterService = awaiterService;
//            _logger = logger;

//            _messageSender = new MessageSender(queueOption.ToConnectionString(), queueOption.QueueName);
//        }

//        /// <summary>
//        /// Call endpoint, expect a single return message
//        /// </summary>
//        /// <param name="messagePayload">payload for message</param>
//        /// <param name="timeout">timeout, null for default</param>
//        /// <returns></returns>
//        public async Task<MessagePayload> Call(MessagePayload messagePayload, TimeSpan? timeout = null)
//        {
//            Message message = messagePayload.ToMessage();

//            _logger.LogTrace($"Sending message: contentType={messagePayload.ContentType}, data.Length {messagePayload.Data?.Length}");

//            await _messageSender!.SendAsync(message);

//            return await WaitForResponse(messagePayload.MessageId, timeout);
//        }

//        public async Task Close()
//        {
//            MessageSender messsageSender = Interlocked.Exchange(ref _messageSender, null!);
//            if (messsageSender != null)
//            {
//                await messsageSender.CloseAsync();
//            }
//        }

//        public async ValueTask DisposeAsync() => await Close();

//        /// <summary>
//        /// Send message, fire and forget
//        /// </summary>
//        /// <param name="messagePayload">payload for message</param>
//        /// <returns></returns>
//        public async Task Send(MessagePayload messagePayload)
//        {
//            if (_messageSender == null || _messageSender?.IsClosedOrClosing == true) return;

//            _logger.LogTrace($"Sending message: contentType={messagePayload.ContentType}, data.Length {messagePayload.Data?.Length}");

//            await _messageSender!.SendAsync(messagePayload.ToMessage());

//        }

//        private Task<MessagePayload> WaitForResponse(Guid messageId, TimeSpan? timeout = null)
//        {
//            var tcs = new TaskCompletionSource<MessagePayload>();
//            _awaiterService.Add(messageId, tcs, timeout);

//            return tcs.Task;
//        }
//    }
//}