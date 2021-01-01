using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Toolbox.Tools;

namespace Toolbox.Azure.Queue
{
    public class QueueReceiver<T> : IAsyncDisposable where T : class
    {
        private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);
        private readonly ILogger<QueueReceiver<T>> _logger;
        private MessageReceiver _messageReceiver;
        private Func<T, Task>? _receiver;

        public QueueReceiver(QueueOption queueOption, string queueName, ILogger<QueueReceiver<T>> logger)
        {
            queueOption.VerifyNotNull(nameof(queueOption));
            queueName.VerifyNotEmpty(nameof(queueName));

            _messageReceiver = new MessageReceiver(queueOption.ToConnectionString(), queueName, ReceiveMode.PeekLock);
            _logger = logger;
        }

        public async ValueTask DisposeAsync() => await Stop();

        public Task Start(Func<T, Task> receiver)
        {
            _messageReceiver.VerifyNotNull("MessageProcessor is not running");
            receiver.VerifyNotNull(nameof(receiver));

            _receiver = receiver;

            // Configure the MessageHandler Options in terms of exception handling, number of concurrent messages to deliver etc.
            var messageHandlerOptions = new MessageHandlerOptions(x => ExceptionReceivedHandler(x))
            {
                // Maximum number of Concurrent calls to the callback `ProcessMessagesAsync`, set to 1 for simplicity.
                // Set it according to how many messages the application wants to process in parallel.
                MaxConcurrentCalls = 1,

                // Indicates whether MessagePump should automatically complete the messages after returning from User Callback.
                // False below indicates the Complete will be handled by the User Callback as in `ProcessMessagesAsync` below.
                AutoComplete = false
            };

            _logger.LogTrace($"{nameof(Start)}: Register message handler");
            _messageReceiver.RegisterMessageHandler(ProcessMessagesAsync, messageHandlerOptions);

            return Task.CompletedTask;
        }

        /// <summary>
        /// Stop receiver
        /// </summary>
        /// <returns></returns>
        public async Task Stop()
        {
            await _lock.WaitAsync();

            try
            {
                MessageReceiver? messageReceiver = Interlocked.Exchange(ref _messageReceiver, null!);
                if (messageReceiver != null)
                {
                    _logger.LogTrace($"{nameof(Stop)}: Stopping");
                    await messageReceiver.CloseAsync();
                }
            }
            finally
            {
                _lock.Release();
            }
        }

        private Task ExceptionReceivedHandler(ExceptionReceivedEventArgs exceptionReceivedEventArgs)
        {
            _logger.LogError(exceptionReceivedEventArgs.Exception, "Message handler encountered an exception");

            var receiverContext = exceptionReceivedEventArgs.ExceptionReceivedContext;

            string msg = new[]
            {
                "Exception context for troubleshooting:",
                $"- Endpoint: {receiverContext.Endpoint}",
                $"- Entity Path: {receiverContext.EntityPath}",
                $"- Executing Action: {receiverContext.Action}",
            }.Aggregate(string.Empty, (a, x) => a += x + Environment.NewLine);

            _logger.LogError(msg);

            return Task.CompletedTask;
        }

        private async Task ProcessMessagesAsync(Message message, CancellationToken token)
        {
            await _lock.WaitAsync();

            try
            {
                if (_messageReceiver == null || message == null || _messageReceiver?.IsClosedOrClosing == true || token.IsCancellationRequested) return;

                // Process the message
                try
                {
                    string json = Encoding.UTF8.GetString(message.Body);
                    T? value = Json.Default.Deserialize<T>(json);

                    if (value == null) throw new ArgumentException($"Failed to parse message, json={json}");

                    await _receiver!(value);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Cannot parse message, {message.MessageId}, CorrelationId={message.CorrelationId}");
                }

                // Complete the message so that it is not received again.
                // This can be done only if the queueClient is created in ReceiveMode.PeekLock mode (which is default).
                await _messageReceiver!.CompleteAsync(message.SystemProperties.LockToken);

                // Note: Use the cancellationToken passed as necessary to determine if the queueClient has already been closed.
                // If queueClient has already been Closed, you may chose to not call CompleteAsync() or AbandonAsync() etc. calls
                // to avoid unnecessary exceptions.
            }
            finally
            {
                _lock.Release();
            }
        }
    }
}