using Bank.sdk.Model;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Azure.Queue;
using Toolbox.Document;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace Bank.sdk.Service;

public class BankClearingQueue
{
    private readonly BankDirectory _bankDirectory;
    private readonly BankOption _clearingOption;
    private readonly ILogger<BankClearingQueue> _logger;

    public BankClearingQueue(BankOption clearingOption, BankDirectory bankDirectory, ILogger<BankClearingQueue> logger)
    {
        _bankDirectory = bankDirectory.VerifyNotNull(nameof(bankDirectory));
        _clearingOption = clearingOption;
        _logger = logger;
    }

    public async Task Send(TrxBatch<TrxRequest> requests, CancellationToken token) => await Send(requests, x => x.ToId, x => x.FromId, token);

    public async Task Send(TrxBatch<TrxRequestResponse> requests, CancellationToken token) => await Send(requests, x => x.Reference.FromId, x => x.Reference.ToId, token);

    private async Task<bool> Send<T>(TrxBatch<T> batch, Func<T, string> getToId, Func<T, string> getFromId, CancellationToken token)
    {
        bool pass = batch.Items
            .Select(x =>
            {
                bool ok = true;
                if (getFromId(x) != _clearingOption.BankName)
                {
                    _logger.LogError("Transaction {trx} is not 'from' bankName={bankName}", x, _clearingOption.BankName);
                    ok = false;
                }

                if (!_bankDirectory.IsBankNameExist(getToId(x)))
                {
                    _logger.LogError("Transaction {trx} 'To' bankName={bankName} is not registered", x, getToId(x));
                    ok = false;
                }

                return ok;
            })
            .All(x => x == true);

        if (pass == false) return false;

        var groups = batch.Items.GroupBy(x => getToId(x));

        foreach (var groupItem in groups)
        {
            DocumentId bankId = (DocumentId)groupItem.Key;
            string bankName = bankId.GetBankName();
            QueueClient<QueueMessage> client = await _bankDirectory.GetClient(bankName, token);

            var trxBatch = new TrxBatch<T>
            {
                Items = new List<T>(groupItem)
            };

            QueueMessage queueMessage = trxBatch.ToQueueMessage();
            await client.Send(queueMessage);

            _logger.LogInformation($"Sent batch to bank={bankName}, count={trxBatch.Items.Count}");
        }

        return true;
    }
}
