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

public class BankTransactionService
{
    private readonly BankOption _bankOption;
    private readonly BankDocumentService _bankAccountService;
    private readonly ILogger<BankTransactionService> _logger;

    public BankTransactionService(BankOption bankOption, BankDocumentService bankAccountService, ILogger<BankTransactionService> logger)
    {
        _bankOption = bankOption.VerifyNotNull(nameof(bankOption));
        _bankAccountService = bankAccountService.VerifyNotNull(nameof(bankAccountService));
        _logger = logger.VerifyNotNull(nameof(logger));
    }

    public async Task<TrxBalance?> GetBalance(DocumentId documentId, CancellationToken token)
    {
        _logger.LogTrace("Getting directoryId={documentId}", documentId);

        if (!documentId.IsBankName(_bankOption.BankName))
        {
            _logger.LogWarning("Bank name is not valid for this service, bankName={bankName}", documentId.GetBankName());
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

        var batchContext = new BatchContext(batch.Items);

        IEnumerable<IGrouping<string, TrxRequest>> groups = batchContext.GetNotProcessedRequests()
            .GroupBy(x => x.GetCreditId().ToId);

        foreach (IGrouping<string, TrxRequest> group in groups)
        {
            if (!((DocumentId)group.Key).IsBankName(_bankOption.BankName))
            {
                _logger.LogWarning("Bank name is not valid for this service, bankName={bankName}", group.Key);
                batchContext.Responses.AddRange(SetResponse(group, TrxStatus.InvalidBank));
                continue;
            }

            BankAccount? bankAccount = await GetBankAccount((DocumentId)group.Key, group, batchContext, token);
            if (bankAccount == null) continue;

            if (!IsWithinBalance(bankAccount, group, batchContext)) continue;

            BankAccount entry = bankAccount with
            {
                Transactions = bankAccount.Transactions
                    .Concat(group.Select(x => new TrxRecord
                    {
                        Type = x.Type,
                        Amount = x.Amount,
                        Properties = x.Properties.ToSafe()
                            .Append($"RequestId={x.Id}")
                            .ToList(),
                    })
                    )
                    .ToList()
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

    private static IEnumerable<TrxRequestResponse> SetResponse(IEnumerable<TrxRequest> requests, TrxStatus status) => requests.Select(x => new TrxRequestResponse
    {
        Reference = x,
        Status = status,
    });

    async Task<BankAccount?> GetBankAccount(DocumentId toId, IEnumerable<TrxRequest> requests, BatchContext batchContext, CancellationToken token)
    {
        BankAccount? bankAccount = await _bankAccountService.Get(toId, token);
        if (bankAccount == null)
        {
            _logger.LogError("Account not found for directoryId={toId}", toId);
            batchContext.Responses.AddRange(SetResponse(requests, TrxStatus.NoAccount));
        }

        return bankAccount;
    }

    private bool IsWithinBalance(BankAccount bankAccount, IEnumerable<TrxRequest> requests, BatchContext batchContext)
    {
        if (bankAccount.Balance() + requests.Balance() >= 0) return true;

        _logger.LogError("No required funds for directoryId={bankAccount.AccountId}", bankAccount.AccountId);
        batchContext.Responses.AddRange(SetResponse(requests, TrxStatus.NoFunds));

        return false;
    }

    private record BatchContext
    {
        public BatchContext(IEnumerable<TrxRequest> trxRequests) => Requests = trxRequests.ToList();

        public IReadOnlyList<TrxRequest> Requests { get; }

        public List<TrxRequestResponse> Responses { get; } = new List<TrxRequestResponse>();

        public IEnumerable<TrxRequest> GetNotProcessedRequests() => Requests
            .Select(x => x.Id)
            .Except(Responses.Select(x => x.Reference.Id))
            .Join(Requests, x => x, x => x.Id, (id, trx) => trx)
            .ToList();
    }
}
