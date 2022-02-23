using Bank.sdk.Model;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Document;
using Toolbox.Extensions;
using Toolbox.Logging;

namespace Bank.sdk.Service;

public class BankTransactionService
{
    private readonly BankAccountService _bankAccountService;
    private readonly ILogger<BankTransactionService> _logger;

    public BankTransactionService(BankAccountService bankAccountService, ILogger<BankTransactionService> logger)
    {
        _bankAccountService = bankAccountService;
        _logger = logger;
    }

    public async Task<TrxBalance?> GetBalance(DocumentId documentId, CancellationToken token)
    {
        _logger.LogTrace($"Getting directoryId={documentId}");

        BankAccount? bankAccount = await _bankAccountService.Get(documentId, token);
        if (bankAccount == null)
        {
            _logger.LogWarning($"Account not found for directoryId={documentId}");
            return null;
        }

        return new TrxBalance
        {
            AccountId = bankAccount.AccountId,
            Balance = bankAccount.Balance()
        };
    }

    public async Task<TrxBatch<TrxRequestResponse>> Set(TrxBatch<TrxRequest> batch, CancellationToken token)
    {
        using var scope = _logger.BeginScopeWithLocation();
        _logger.Trace($"Setting transaction for id={batch.Id}, count={batch.Items.Count}");

        IEnumerable<IGrouping<string, TrxRequest>> groups = batch.Items
            .GroupBy(x => x.AccountId);

        List<TrxRequestResponse> responses = new();

        foreach (IGrouping<string, TrxRequest> group in groups)
        {
            BankAccount? bankAccount = await _bankAccountService.Get((DocumentId)group.Key, token);
            if (bankAccount == null)
            {
                _logger.LogError($"Account not found for directoryId={group.Key}");
                responses.AddRange(group.Select(x => new TrxRequestResponse
                {
                    ReferenceId = x.Id,
                    Status = TrxStatus.NoAccount,
                }));

                continue;
            }

            if( bankAccount.Balance < group.Sum(x => x.Type switch { TrxType.Credit => x.Amount

            BankAccount entry = bankAccount with
            {
                Transactions = bankAccount.Transactions
                    .Concat(group)
                    .OrderBy(x => x.Date)
                    .Select(x => new TrxRecord
                    {
                        Type = x.Type,
                        Amount = x.Amount,
                        Properties = x.Properties.ToSafe()
                            .Append($"RequestId={x.Id}")
                            .ToList(),
                    }))
                    .ToList()
            };

            _logger.LogTrace($"Add transactions to accountId={group.Key}");
            await _bankAccountService.Set(entry, token);

            responses.AddRange(group.Select(x => new TrxRequestResponse
            {
                ReferenceId = x.Id,
                Status = TrxStatus.Success
            }));
        }

        return new TrxBatch<TrxRequestResponse>
        {
            Items = responses,
        };

decimal Balance(IEnumerable<TrxRequest> trxRequests) => trxRequests.Sum(x => x.Type switch
{
    TrxType.Credit => x.Amount,
    TrxType.Debit => 0 - x.Amount,

    _ => throw new ArgumentException
});
    }

    public async Task<TrxStatus> Set(ClearingRequest clearingRequest, CancellationToken token)
    {
        using var scope = _logger.BeginScopeWithLocation();
        _logger.Trace($"Setting clearing request id={clearingRequest.Id}");


        BankAccount? bankAccount = await _bankAccountService.Get((DocumentId)clearingRequest.FromId, token);
        if (bankAccount == null)
        {
            _logger.LogError($"Account not found for directoryId={clearingRequest.ToId}");
            return TrxStatus.NoAccount;
        }

        if (bankAccount.Balance() < clearingRequest.Amount) return TrxStatus.NoFunds;

        BankAccount entry = bankAccount with
        {
            Transactions = bankAccount.Transactions
                .Append(new TrxRecord
                {
                    Type = TrxType.Debit,
                    Amount = clearingRequest.Amount,
                    Properties = clearingRequest.Properties.ToSafe()
                        .Append($"RequestId={clearingRequest.Id}")
                        .ToList(),
                })
                .OrderBy(x => x.Date)
                .ToList()
        };

        _logger.LogTrace($"Add clearing request to accountId={clearingRequest.FromId}");
        await _bankAccountService.Set(entry, token);
        return TrxStatus.FundsWithdrawn;
    }
}
