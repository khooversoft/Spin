using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace Toolbox.Azure.Queue;

public class QueueReceiver<T> : IQueueReceiver, IAsyncDisposable where T : class
{
    private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);
    private readonly ILogger<QueueReceiver<T>> _logger;
    private readonly QueueReceiverOption<T> _queueReceiver;
    private ServiceBusClient? _serviceBusClient;
    private ServiceBusProcessor? _serviceBusProcessor;

    public QueueReceiver(QueueReceiverOption<T> queueReceiver, ILogger<QueueReceiver<T>> logger)
    {
        _queueReceiver = queueReceiver.NotNull(nameof(queueReceiver));
        _logger = logger.NotNull(nameof(logger));

        var options = new ServiceBusProcessorOptions
        {
            AutoCompleteMessages = _queueReceiver.AutoComplete,
            MaxConcurrentCalls = _queueReceiver.MaxConcurrentCalls,
        };

        _serviceBusClient = new ServiceBusClient(queueReceiver.QueueOption.ToConnectionString());
        _serviceBusProcessor = _serviceBusClient.CreateProcessor(queueReceiver.QueueOption.QueueName, options);
        _serviceBusProcessor.ProcessMessageAsync += MessageHandler;
        _serviceBusProcessor.ProcessErrorAsync += ErrorHandler;
    }

    public async ValueTask DisposeAsync() => await Stop();

    public async Task Start()
    {
        _serviceBusClient.NotNull("MessageProcessor is not running");

        await _serviceBusProcessor!.StartProcessingAsync();
        _logger.LogInformation("Queue receiver started");
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
            ServiceBusProcessor? processor = Interlocked.Exchange(ref _serviceBusProcessor, null!);
            if (processor != null)
            {
                _logger.LogTrace("Stopping receiver processor");
                await processor.CloseAsync();
            }

            ServiceBusClient? client = Interlocked.Exchange(ref _serviceBusClient, null!);
            if (client != null)
            {
                _logger.LogTrace("Closing queue client");
                await client.DisposeAsync();
            }
        }
        finally
        {
            _lock.Release();
        }
    }

    private async Task MessageHandler(ProcessMessageEventArgs args)
    {
        T? value = null;

        // Process the message
        try
        {
            _logger.LogTrace($"Starting processing message {args.Message.MessageId}");

            string json = Encoding.UTF8.GetString(args.Message.Body.ToArray());
            value = Json.Default.Deserialize<T>(json);

            if (value == null) throw new ArgumentException($"Failed to parse message, json={json}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Cannot parse message, {args.Message.MessageId}, CorrelationId={args.Message.CorrelationId}", args.Message.MessageId, args.Message.CorrelationId);
            return;
        }

        try
        {
            bool status = await _queueReceiver.Receiver(value);

            // Complete the message so that it is not received again.
            // This can be done only if the queueClient is created in ReceiveMode.PeekLock mode (which is default).
            if (status)
            {
                if (!_queueReceiver.AutoComplete)
                {
                    _logger.LogTrace("Complete queued message {args.Message.MessageId}", args.Message.MessageId);
                    await args.CompleteMessageAsync(args.Message);
                }
            }
            else
            {
                _logger.LogTrace("Receiver return false, message is sent to the dead letter queue {args.Message.MessageId}", args.Message.MessageId);
                await args.DeadLetterMessageAsync(args.Message);
            }

            _logger.LogTrace("Completed message {args.Message.MessageId}", args.Message.MessageId);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Processing message failed, messageId={args.Message.MessageId}, CorrelationId={args.Message.CorrelationId}",
                args.Message.MessageId,
                args.Message.CorrelationId
                );
        }
    }

    private Task ErrorHandler(ProcessErrorEventArgs args)
    {
        _logger.LogError(args.Exception, "Message handler encountered an exception");

        string msg = new[]
        {
            "Exception context for troubleshooting:",
            $"- Endpoint: {args.EntityPath}",
            $"- FullyQualifiedNamespace: {args.FullyQualifiedNamespace}",
            $"- ErrorSource: {args.ErrorSource}",
        }.Join(Environment.NewLine);

        _logger.LogError(msg);

        return Task.CompletedTask;
    }
}
