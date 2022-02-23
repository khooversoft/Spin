using Bank.sdk.Model;
using Bank.sdk.Service;
using System.Collections.Concurrent;
using Toolbox.Azure.Queue;
using Toolbox.Logging;
using Toolbox.Tools;

namespace BankApi.Service;

public class ClearningReceiver : IHostedService
{
    private readonly BankClearingService _bankClearingService;
    private readonly BankTransactionService _bankTransactionService;
    private readonly ILogger<ClearningReceiver> _logger;
    private CancellationTokenSource? _tokenSource = new CancellationTokenSource();

    public ClearningReceiver(BankClearingService bankClearingService, BankTransactionService bankTransactionService, ILogger<ClearningReceiver> logger)
    {
        _bankClearingService = bankClearingService;
        _bankTransactionService = bankTransactionService;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken token)
    {
        _tokenSource ??= new CancellationTokenSource();

        _logger.Information("Starting clearing service");

        await _bankClearingService.Start(Receiver, token);
        _logger.Information("Clearing service started");
    }

    public async Task StopAsync(CancellationToken token)
    {
        await _bankClearingService.Stop();
        Interlocked.Exchange(ref _tokenSource, null!)?.Cancel();
    }

    private async Task<bool> Receiver(QueueMessage queueMessage)
    {
        if (_tokenSource?.IsCancellationRequested == true) return false;

        switch (queueMessage.ContentType)
        {
            case nameof(ClearingRequest):
                ClearingRequest clearingRequest = queueMessage.GetContent<ClearingRequest>();
                TrxStatus status = await _bankTransactionService.Set(clearingRequest, _tokenSource!.Token);

                ClearingRequestResponse response = clearingRequest.ToClearingRequestResponse(ClearingStatus.

                return status == TrxStatus.Success;

            case nameof(ClearingRequestResponse):
                break;

            default:
                _logger.Error($"Unknown contentType={queueMessage.ContentType}");
                return false;
        }
    }
}
