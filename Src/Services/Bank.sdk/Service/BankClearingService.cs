﻿using Bank.sdk.Model;
using Directory.sdk;
using Directory.sdk.Client;
using Directory.sdk.Service;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Application;
using Toolbox.Azure.Queue;
using Toolbox.Document;
using Toolbox.Extensions;
using Toolbox.Logging;
using Toolbox.Tools;

namespace Bank.sdk.Service;

public class BankClearingService
{
    private readonly BankDirectory _bankDirectory;
    private readonly BankTransactionService _bankTransactionService;
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger<BankClearingService> _logger;

    private int _lock = 0;
    private QueueReceiver<QueueMessage>? _receiver;
    private CancellationTokenSource? _cancellationTokenSource;

    public BankClearingService(BankDirectory bankDirectory, BankTransactionService bankTransactionService, ILoggerFactory loggerFactory)
    {
        _bankDirectory = bankDirectory;
        _bankTransactionService = bankTransactionService;
        _loggerFactory = loggerFactory;

        _logger = _loggerFactory.CreateLogger<BankClearingService>();
    }

    public async Task Start(CancellationToken token)
    {
        int lockState = Interlocked.CompareExchange(ref _lock, 1, 0);
        if (lockState == 1) return;

        _logger.Information("Starting receiver");

        try
        {
            if (_receiver != null) return;

            QueueOption queueOption = await _bankDirectory.GetQueueOption(token);

            var receiverOption = new QueueReceiverOption<QueueMessage>
            {
                QueueOption = queueOption,
                Receiver = Receiver
            };

            _receiver = new QueueReceiver<QueueMessage>(receiverOption, _loggerFactory.CreateLogger<QueueReceiver<QueueMessage>>());
            _receiver.Start();
        }
        finally
        {
            _lock = 0;
        }
    }

    public async Task Stop()
    {
        Interlocked.Exchange(ref _cancellationTokenSource, null)?.Cancel();

        QueueReceiver<QueueMessage>? receiver = Interlocked.Exchange(ref _receiver, null);
        if (receiver == null) return;

        await receiver.Stop();
    }

    private async Task<bool> Receiver(QueueMessage queueMessage)
    {
        if (_cancellationTokenSource == null || _cancellationTokenSource.Token.IsCancellationRequested == true) return false;

        switch (queueMessage.ContentType)
        {
            case nameof(TrxRequest):
                TrxBatch<TrxRequest> requests = queueMessage.GetContent<TrxBatch<TrxRequest>>();
                await ProcessTrxRequests(requests, _cancellationTokenSource.Token);
                return true;

            case nameof(TrxRequestResponse):
                TrxBatch<TrxRequestResponse> responses = queueMessage.GetContent<TrxBatch<TrxRequestResponse>>();
                await ProcessTrxResponses(responses, _cancellationTokenSource.Token);
                return true;

            default:
                _logger.Error($"Unknown contentType={queueMessage.ContentType}");
                return false;
        }
    }

    private async Task ProcessTrxRequests(TrxBatch<TrxRequest> requests, CancellationToken token)
    {
        _logger.Information($"Processing TrxRequest batch, batchId={requests.Id}");

        TrxBatch<TrxRequestResponse> responses = await _bankTransactionService.Set(requests, token);
        await _bankDirectory.Send(responses, token);
    }

    private async Task ProcessTrxResponses(TrxBatch<TrxRequestResponse> responses, CancellationToken token)
    {
        _logger.Information($"Processing TrxResponses batch, batchId={responses.Id}");

        var batch = new TrxBatch<TrxRequest>
        {
            Items = responses.Items.Select(x => new TrxRequest
            {
                FromId = x.Reference.ToId,
                ToId = x.Reference.ToId,
                Type = x.Reference.Type == TrxType.Credit ? TrxType.Debit : TrxType.Credit,
                Amount = x.Reference.Amount,
                Properties = x.Reference.Properties
            }).ToList()
        };

        await _bankTransactionService.Set(batch, token);
    }
}
