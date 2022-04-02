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
public class BankClearingService
{
    private readonly BankClearingQueue _bankClearingQueue;
    private readonly ILogger<BankClearingService> _logging;
    private readonly BankTransactionService _bankTransactionService;
    private readonly BankOption _bankOption;

    public BankClearingService(BankOption bankOption, BankClearingQueue bankClearingQueue, BankTransactionService bankTransactionService, ILogger<BankClearingService> logging)
    {
        _bankOption = bankOption.VerifyNotNull(nameof(bankOption));
        _bankClearingQueue = bankClearingQueue.VerifyNotNull(nameof(bankClearingQueue));
        _bankTransactionService = bankTransactionService.VerifyNotNull(nameof(bankTransactionService));
        _logging = logging.VerifyNotNull(nameof(logging));
    }

    public async Task Process(TrxBatch<TrxRequest> requests, CancellationToken token)
    {
        Verify(requests);

        TrxBatch<TrxRequestResponse> response = await _bankTransactionService.Set(requests, token);

        await _bankClearingQueue.Send(response, token);
    }

    public async Task Process(TrxBatch<TrxRequestResponse> responses, CancellationToken token)
    {
        Verify(responses.Items.Select(x => x.Reference));

        var creditTrx = new TrxBatch<TrxRequest>
        {
            Items = responses.Items
                .Where(x => _bankOption.IsBankName(x.Reference.ToId) && x.Status == TrxStatus.Success)
                .Select(x => x.Reference)
                .ToList()
        };

        TrxBatch<TrxRequestResponse> response = await _bankTransactionService.Set(creditTrx, token);

        await _bankClearingQueue.Send(response, token);
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

        TrxBatch<TrxRequestResponse> response = await _bankTransactionService.Set(debitTrx, token);

        // Only send pull and valid push
        var sendRequest = new TrxBatch<TrxRequest>
        {
            Items = requests.Items
                .Where(x => !_bankOption.IsBankName(x.FromId) || response.Items.Where(y => x.Id == y.Id && y.Status == TrxStatus.Success).Any())
                .ToList()
        };

        await _bankClearingQueue.Send(sendRequest, token);

        return response;
    }


    private void Verify(TrxBatch<TrxRequest> batch)
    {
        batch.VerifyNotNull(nameof(batch));
        Verify(batch.Items);
    }

    private void Verify(IEnumerable<TrxRequest> requests)
    {

        requests
            .Select(x => (x, isVerify: x.IsVerify()))
            .Where(x => !x.isVerify.Pass)
            .Select(x => $"{x.x} {x.isVerify.Message}")
            .Func(x => x.Join(", "))
            .Action(x =>
            {
                if (!x.IsEmpty())
                {
                    _logging.LogError("TrxBatch has errors: {error}", x);
                    throw new ArgumentException($"TrxBatch has errors: {x}");
                }
            });
    }

    private void VerifyBank(TrxBatch<TrxRequest> requests) => requests.Items
            .Where(x => !_bankOption.IsBankName(x.ToId) && !_bankOption.IsBankName(x.FromId))
            .Select(x => $"{x} transaction is not associated with bank {_bankOption.BankName}")
            .Func(x => x.Join(", "))
            .Action(x =>
            {
                if (!x.IsEmpty())
                {
                    _logging.LogError("TrxBatch has errors: {error}", x);
                    throw new ArgumentException($"TrxBatch has errors: {x}");
                }
            });

    public void Verify(TrxBatch<TrxRequestResponse> responses)
    {
        responses.VerifyNotNull(nameof(responses));

    }
}
