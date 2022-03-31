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
        Verify(requests, false);

        TrxBatch<TrxRequestResponse> response = await _bankTransactionService.Set(requests, token);

        await _bankClearingQueue.Send(response, token);
    }


    public async Task Send(TrxBatch<TrxRequest> requests, CancellationToken token)
    {
        Verify(requests, true);

        await _bankClearingQueue.Send(requests, token);
    }


    private void Verify(TrxBatch<TrxRequest> requests, bool sendMode)
    {
        requests.VerifyNotNull(nameof(requests));

        bool isBank(string id) => ((DocumentId)id).IsBankName(_bankOption.BankName);

        requests.Items
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

        requests.Items
            .Select(x => (x, isBank: isBank(x.GetCreditId().ToId)))
            .Where(x => !x.isBank)
            .Select(x => $"{x.x} is not {(sendMode ? "sender" : "receiver")} bank {_bankOption.BankName}")
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
}
