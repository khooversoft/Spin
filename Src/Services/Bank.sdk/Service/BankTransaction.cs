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
using Toolbox.Tools;

namespace Bank.sdk.Service;

public class BankTransaction
{
    private readonly BankOption _bankOption;
    private readonly BankDocument _bankAccountService;
    private readonly ILogger<BankTransaction> _logger;

    internal BankTransaction(BankOption bankOption, BankDocument bankAccountService, ILogger<BankTransaction> logger)
    {
        _bankOption = bankOption.VerifyNotNull(nameof(bankOption));
        _bankAccountService = bankAccountService.VerifyNotNull(nameof(bankAccountService));
        _logger = logger.VerifyNotNull(nameof(logger));
    }

    public async Task<TrxBalance?> GetBalance(DocumentId documentId, CancellationToken token)
    {
        _logger.LogTrace("Getting directoryId={documentId}", documentId);

        BankAccountId? bankAccountId = documentId.ToBankAccountId();
        if (bankAccountId == null)
        {
            _logger.LogWarning("Bank account id is not valid, id={id}", documentId);
            return null;
        }

        if (!bankAccountId.BankName.Equals(_bankOption.BankName, StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("Bank name is not valid for this service, bankName={bankName}, can only be {serviceBankName}", bankAccountId.BankName, _bankOption.BankName);
            return null;
        }

        BankAccount? bankAccount = await _bankAccountService.Get(documentId, token);
        if (bankAccount == null)
        {
            _logger.LogWarning("Account not found for directoryId={documentId}", documentId);
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
        _logger.LogTrace("Setting transaction for id={batch.Id}, count={batch.Items.Count}", batch.Id, batch.Items.Count);

        var batchContext = new BatchContext(batch.Items, _bankOption);

        var groups = batchContext.GetNotProcessedRequests()
            .GroupBy(x =>_bankOption.GetLocalId(x));

        foreach (IGrouping<string, TrxRequest> group in groups)
        {
            BankAccount? bankAccount = await GetBankAccount((DocumentId)group.Key, group, batchContext, token);
            if (bankAccount == null) continue;

            var appendTrx = group.Select(x => new TrxRecord
            {
                Type = x.ToId.EqualsIgnoreCase(group.Key) ? TrxType.Credit : TrxType.Debit,
                Amount = x.Amount,
                Properties = x.Properties.ToSafe().ToList(),
                TrxRequestId = x.Id,
            }).ToList();

            if (!IsWithinBalance(bankAccount, group, appendTrx, batchContext)) continue;

            BankAccount entry = bankAccount with
            {
                Transactions = bankAccount.Transactions
                    .Concat(appendTrx)
                    .ToList(),
                Requests = bankAccount.Requests
                    .Concat(group)
                    .ToList(),
            };

            _logger.LogInformation("Add transactions to accountId={group.Key}", group.Key);
            await _bankAccountService.Set(entry, token);

            batchContext.Responses.AddRange(group.Select(x => new TrxRequestResponse
            {
                Reference = x,
                Status = TrxStatus.Success
            }));
        }

        return new TrxBatch<TrxRequestResponse>
        {
            Items = batchContext.Responses.ToList(),
        };
    }

    public async Task RecordResponses(TrxBatch<TrxRequestResponse> batch, CancellationToken token)
    {
        List<TrxRequestResponse> updateItems = batch.Items
            .Where(x => _bankOption.IsForLocalHost(x.Reference))
            .Select(x => x)
            .ToList();

        var groups = updateItems.GroupBy(x => _bankOption.GetLocalId(x.Reference));

        foreach (var group in groups)
        {
            BankAccount? bankAccount = await _bankAccountService.Get((DocumentId)group.Key, token);
            if (bankAccount == null || group.Count() == 0) continue;

            bankAccount = bankAccount with
            {
                Responses = bankAccount.Responses.Concat(group).ToList()
            };

            await _bankAccountService.Set(bankAccount, token);
        }
    }


    private static IEnumerable<TrxRequestResponse> SetResponse(IEnumerable<TrxRequest> requests, TrxStatus status) => requests.Select(x => new TrxRequestResponse
    {
        Reference = x,
        Status = status,
    });

    private async Task<BankAccount?> GetBankAccount(DocumentId toId, IEnumerable<TrxRequest> requests, BatchContext batchContext, CancellationToken token)
    {
        BankAccount? bankAccount = await _bankAccountService.Get(toId, token);
        if (bankAccount == null)
        {
            _logger.LogError("Account not found for directoryId={toId}", toId);
            batchContext.Responses.AddRange(SetResponse(requests, TrxStatus.NoAccount));
        }

        return bankAccount;
    }

    private bool IsWithinBalance(BankAccount bankAccount, IEnumerable<TrxRequest> requests, IEnumerable<TrxRecord> records, BatchContext batchContext)
    {
        if (bankAccount.Balance() + records.Balance() >= 0) return true;

        _logger.LogError("No required funds for directoryId={bankAccount.AccountId}", bankAccount.AccountId);
        batchContext.Responses.AddRange(SetResponse(requests, TrxStatus.NoFunds));

        return false;
    }

    private record BatchContext
    {
        public BatchContext(IEnumerable<TrxRequest> trxRequests, BankOption bankOption)
        {
            var requests = trxRequests
                .Select(x => (source: x, verify: x.IsVerify() && bankOption.IsForLocalHost(x)))
                .ToList();

            Requests = requests
                .Where(x => x.verify)
                .Select(x => x.source)
                .ToList();

            var invalid = requests
                .Where(x => !x.verify)
                .Select(x => new TrxRequestResponse
                {
                    Reference = x.source,
                    Status = TrxStatus.InvalidRequest
                });

            Responses.AddRange(invalid);
        }

        public IReadOnlyList<TrxRequest> Requests { get; }

        public List<TrxRequestResponse> Responses { get; } = new List<TrxRequestResponse>();

        public IEnumerable<TrxRequest> GetNotProcessedRequests() => Requests
            .Select(x => x.Id)
            .Except(Responses.Select(x => x.Id))
            .Join(Requests, x => x, x => x.Id, (id, trx) => trx)
            .ToList();
    }
}
