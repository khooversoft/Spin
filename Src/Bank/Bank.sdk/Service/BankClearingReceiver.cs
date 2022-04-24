using Bank.Abstractions.Model;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Spin.Common.Model;
using Spin.Common.Services;
using Toolbox.Azure.Queue;
using Toolbox.Tools;

namespace Bank.sdk.Service;

public class BankClearingReceiver : IHostedService
{
    private readonly BankDirectory _bankDirectory;
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger<BankClearingReceiver> _logger;
    private readonly BankClearing _bankClearingService;
    private int _lock = 0;
    private QueueReceiver<QueueMessage>? _receiver;
    private CancellationTokenSource? _cancellationTokenSource;

    internal BankClearingReceiver(BankClearing bankClearingService, BankDirectory bankDirectory, ILoggerFactory loggerFactory)
    {
        _bankClearingService = bankClearingService.VerifyNotNull(nameof(bankClearingService));
        _bankDirectory = bankDirectory.VerifyNotNull(nameof(bankDirectory));
        _loggerFactory = loggerFactory.VerifyNotNull(nameof(loggerFactory));

        _logger = _loggerFactory.CreateLogger<BankClearingReceiver>();
    }

    public async Task StartAsync(CancellationToken token)
    {
        int lockState = Interlocked.CompareExchange(ref _lock, 1, 0);
        if (lockState == 1) return;

        _logger.LogInformation("Starting receiver");

        try
        {
            if (_receiver != null) return;

            _cancellationTokenSource = new CancellationTokenSource();
            QueueOption queueOption = await _bankDirectory.GetQueueOption(token);

            var receiverOption = new QueueReceiverOption<QueueMessage>
            {
                QueueOption = queueOption,
                Receiver = Receiver
            };

            _receiver = new QueueReceiver<QueueMessage>(receiverOption, _loggerFactory.CreateLogger<QueueReceiver<QueueMessage>>());
            await _receiver.Start();
        }
        finally
        {
            _lock = 0;
        }
    }

    public async Task StopAsync(CancellationToken token)
    {
        Interlocked.Exchange(ref _cancellationTokenSource, null)?.Cancel();

        QueueReceiver<QueueMessage>? receiver = Interlocked.Exchange(ref _receiver, null);
        if (receiver == null) return;

        await receiver.Stop();
    }

    private async Task<bool> Receiver(QueueMessage queueMessage)
    {
        if (_cancellationTokenSource == null || _cancellationTokenSource.Token.IsCancellationRequested == true) return false;

        _logger.LogTrace("Receive queue message, id={id}, contentType={contentType}", queueMessage.MessageId, queueMessage.ContentType);

        switch (queueMessage.ContentType)
        {
            case nameof(TrxRequest):
                TrxBatch<TrxRequest> requests = queueMessage.GetContent<TrxBatch<TrxRequest>>();
                await _bankClearingService.Process(requests, _cancellationTokenSource.Token);
                return true;

            case nameof(TrxRequestResponse):
                TrxBatch<TrxRequestResponse> responses = queueMessage.GetContent<TrxBatch<TrxRequestResponse>>();
                await _bankClearingService.Process(responses, _cancellationTokenSource.Token);
                return true;

            default:
                _logger.LogError($"Unknown contentType={queueMessage.ContentType}");
                return false;
        }
    }
}
