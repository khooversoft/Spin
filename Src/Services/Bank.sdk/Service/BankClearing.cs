using Bank.sdk.Model;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Document;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace Bank.sdk.Service;

/// <summary>
/// Bank clearing service is responsible for processing transaction from the sending bank's perspective, the "requesting bank".
/// 
/// Credit (Push) - Money is pushed from the "from" bank/account to the "to" bank/account.
/// Debit (Pull) - Money is pulled from the "to" bank/account to the "from" bank/account.
/// 
/// Send:
///     Send transactions to each of the "to" queues
/// 
/// </summary>
public class BankClearing
{
    private readonly BankClearingQueue _bankClearingQueue;
    private readonly ILogger<BankClearing> _logger;
    private readonly BankTransaction _bankTransactionService;
    private readonly BankOption _bankOption;

    internal BankClearing(BankOption bankOption, BankClearingQueue bankClearingQueue, BankTransaction bankTransactionService, ILogger<BankClearing> logger)
    {
        _bankOption = bankOption.VerifyNotNull(nameof(bankOption));
        _bankClearingQueue = bankClearingQueue.VerifyNotNull(nameof(bankClearingQueue));
        _bankTransactionService = bankTransactionService.VerifyNotNull(nameof(bankTransactionService));
        _logger = logger.VerifyNotNull(nameof(logger));
    }

    public async Task<TrxBatch<TrxRequestResponse>> Send(TrxBatch<TrxRequest> requests, CancellationToken token)
    {
        Verify(requests);
        VerifyBank(requests);

        // Debit accounts where trx.FromId == bank
        var debitTrx = new TrxBatch<TrxRequest>
        {
            Items = requests.Items
                .Where(x => _bankOption.IsBankName(x.FromId))
                .ToList()
        };

        TrxBatch<TrxRequestResponse>? response = null;

        if (debitTrx.Items.Count > 0)
        {
            response = await _bankTransactionService.Set(debitTrx, token);
        }

        // Only send pull and valid push
        var sendRequest = new TrxBatch<TrxRequest>
        {
            Items = requests.Items
                .ToList()
        };

        await _bankClearingQueue.Send(sendRequest, token);

        return response ?? new TrxBatch<TrxRequestResponse>();
    }

    internal async Task Process(TrxBatch<TrxRequest> requests, CancellationToken token)
    {
        Verify(requests);

        _logger.LogTrace("Processing TrxBatch<TrxRequest>, id={id}", requests.Id);

        TrxBatch<TrxRequestResponse> response = await _bankTransactionService.Set(requests, token);

        _logger.LogTrace("Queuing responses for TrxBatch<TrxRequest>, id={id}", requests.Id);
        await _bankClearingQueue.Send(response, token);
    }

    internal async Task Process(TrxBatch<TrxRequestResponse> responses, CancellationToken token)
    {
        Verify(responses.Items.Select(x => x.Reference));

        _logger.LogTrace("Processing TrxBatch<TrxRequestResponse>, id={id}", responses.Id);

        var creditTrx = new TrxBatch<TrxRequest>
        {
            Items = responses.Items
                .Where(x => _bankOption.IsBankName(x.Reference.ToId) && x.Status == TrxStatus.Success)
                .Select(x => x.Reference)
                .ToList()
        };

        if (creditTrx.Items.Any())
        {
            _logger.LogTrace("Processing credit's from TrxBatch<TrxRequestResponse>, messageId={messageId}", responses.Id);
            await _bankTransactionService.Set(creditTrx, token);
        }

        _logger.LogTrace("Recording TrxBatch<TrxRequestResponse>, id={id}", responses.Id);
        await _bankTransactionService.RecordResponses(responses, token);
    }

    private void Verify(TrxBatch<TrxRequest> batch)
    {
        batch.VerifyNotNull(nameof(batch));
        Verify(batch.Items);
    }

    private void Verify(IEnumerable<TrxRequest> requests)
    {
        if (!requests.All(x => x.IsVerify()))
        {
            const string msg = "TrxBatch has errors";
            _logger.LogError(msg);
            throw new ArgumentException(msg);
        }
    }

    private void VerifyBank(TrxBatch<TrxRequest> requests) => requests.Items
        .Where(x => !_bankOption.IsBankName(x.ToId) && !_bankOption.IsBankName(x.FromId))
        .Select(x => $"{x} transaction is not associated with bank {_bankOption.BankName}")
        .Func(x => x.Join(", "))
        .Action(x =>
        {
            if (!x.IsEmpty())
            {
                _logger.LogError("TrxBatch has errors: {error}", x);
                throw new ArgumentException($"TrxBatch has errors: {x}");
            }
        });
}
