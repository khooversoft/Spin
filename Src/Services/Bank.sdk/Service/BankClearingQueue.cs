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

    internal BankClearingQueue(BankOption clearingOption, BankDirectory bankDirectory, ILogger<BankClearingQueue> logger)
    {
        _bankDirectory = bankDirectory.VerifyNotNull(nameof(bankDirectory));
        _clearingOption = clearingOption;
        _logger = logger;
    }

    public async Task Send(TrxBatch<TrxRequest> requests, CancellationToken token) => await Send(requests, x => x.ToId.ToDocumentId(), x => x.FromId.ToDocumentId(), token);

    public async Task Send(TrxBatch<TrxRequestResponse> requests, CancellationToken token) => await Send(requests, x => x.Reference.FromId.ToDocumentId(), x => x.Reference.ToId.ToDocumentId(), token);

    private async Task<bool> Send<T>(TrxBatch<T> batch, Func<T, DocumentId> getToId, Func<T, DocumentId> getFromId, CancellationToken token)
    {
        batch.VerifyNotNull(nameof(batch));
        getToId.VerifyNotNull(nameof(getToId));
        getFromId.VerifyNotNull(nameof(getFromId));

        bool verify(T subject) =>
            getFromId(subject).GetBankName().Equals(_clearingOption.BankName, StringComparison.OrdinalIgnoreCase) &&
            _bankDirectory.IsBankNameExist(getToId(subject).GetBankName());

        if (!batch.Items.All(x => verify(x)))
        {
            _logger.LogError("One or more transactions has errors in to or from id");
            return false;
        }

        var groups = batch.Items.GroupBy(x => getToId(x).ToString());

        foreach (var groupItem in groups)
        {
            string bankName = groupItem.Key.ToDocumentId().GetBankName();
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
