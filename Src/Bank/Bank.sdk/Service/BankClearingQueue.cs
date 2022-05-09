using Bank.Abstractions.Model;
using Microsoft.Extensions.Logging;
using Toolbox.Abstractions;
using Toolbox.Azure.Queue;
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
        _bankDirectory = bankDirectory.NotNull(nameof(bankDirectory));
        _clearingOption = clearingOption;
        _logger = logger;
    }

    public async Task Send(TrxBatch<TrxRequest> requests, CancellationToken token) => await Send(requests, x => x.ToId, x => x.FromId, token);

    public async Task Send(TrxBatch<TrxRequestResponse> requests, CancellationToken token) => await Send(requests, x => x.Reference.ToId, x => x.Reference.FromId, token);

    private async Task Send<T>(TrxBatch<T> batch, Func<T, string> getToId, Func<T, string> getFromId, CancellationToken token)
    {
        batch.NotNull(nameof(batch));
        getToId.NotNull(nameof(getToId));
        getFromId.NotNull(nameof(getFromId));

        if (Verify(batch, getToId, getFromId))
        {
            throw new ArgumentException("One or more transactions has errors");
        }

        string getSendToId(T item) => _clearingOption.IsBankName(getToId(item)) ? getFromId(item) : getToId(item);

        var groups = batch.Items.GroupBy(x => getSendToId(x));

        foreach (var groupItem in groups)
        {
            string bankName = groupItem.Key.ToDocumentId().GetBankName();
            QueueClient<QueueMessage> client = await _bankDirectory.GetClient(bankName, token);

            var trxBatch = new TrxBatch<T>
            {
                Items = new List<T>(groupItem)
            };

            QueueMessage queueMessage = trxBatch.ToQueueMessage(typeof(T).Name);
            await client.Send(queueMessage, token);

            _logger.LogInformation("Sent batch to bank={bankName}, count={trxBatch.Items.Count}", bankName, trxBatch.Items.Count);
        }
    }

    private bool Verify<T>(TrxBatch<T> batch, Func<T, string> getToId, Func<T, string> getFromId)
    {
        Func<T, bool>[] tests = new Func<T, bool>[]
        {
            x => !getFromId(x).EqualsIgnoreCase(getFromId(x)),

            x => _clearingOption.IsBankName(getToId(x)) || _clearingOption.IsBankName(getFromId(x)),

            x => _bankDirectory.IsBankNameExist(((DocumentId)getToId(x)).GetBankName()) ||
                    _bankDirectory.IsBankNameExist(((DocumentId)getFromId(x)).GetBankName()),
        };

        bool isVerify(T item) => tests.All(x => x(item));

        return batch.Items.All(x => isVerify(x));
    }
}
