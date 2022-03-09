using Bank.sdk.Model;
using Directory.sdk;
using Directory.sdk.Client;
using Directory.sdk.Service;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Azure.Queue;
using Toolbox.Document;
using Toolbox.Extensions;
using Toolbox.Logging;
using Toolbox.Tools;

namespace Bank.sdk.Service;

public class BankDirectory
{
    private readonly SemaphoreSlim _asyncLock = new SemaphoreSlim(1, 1);
    private readonly DirectoryClient _directoryClient;
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger<BankDirectory> _logger;
    private readonly Dictionary<string, BankDetail> _banks = new(StringComparer.OrdinalIgnoreCase);
    private readonly ClearingOption _clearingOption;

    public BankDirectory(ClearingOption clearingOption, DirectoryClient directoryClient, ILoggerFactory loggerFactory)
    {
        _clearingOption = clearingOption;
        _directoryClient = directoryClient;
        _loggerFactory = loggerFactory;
        _logger = loggerFactory.CreateLogger<BankDirectory>();
    }

    public async Task<QueueClient<QueueMessage>> GetClient(string bankName, CancellationToken token)
    {
        bankName.VerifyNotEmpty(nameof(bankName));

        await LoadDirectory(token);

        _banks.TryGetValue(bankName, out BankDetail? bankDetail)
            .VerifyAssert(x => x == true, $"No {bankName} was found");

        QueueOption option = await GetQueueOption((DocumentId)bankDetail!.QueueId, token);

        return new QueueClient<QueueMessage>(option, _loggerFactory.CreateLogger<QueueClient<QueueMessage>>());
    }

    public async Task Send(TrxBatch<TrxRequest> requests, CancellationToken token) => await Send(requests, x => x.ToId, x => x.FromId, token);

    public async Task Send(TrxBatch<TrxRequestResponse> requests, CancellationToken token) => await Send(requests, x => x.Reference.FromId, x => x.Reference.ToId, token);

    private async Task Send<T>(TrxBatch<T> batch, Func<T, string> getToId, Func<T, string> getFromId, CancellationToken token)
    {
        string negativeTestResults = batch.Items
            .SelectMany(x => new[] {
                    (Pass: getFromId(x) == _clearingOption.BankName, Message: $"Transaction {x} is not 'from' bankName={_clearingOption.BankName}"),
                    (Pass: _banks.ContainsKey(getToId(x)), Message: $"Transaction {x} is not 'from' bankName={_clearingOption.BankName}"),
                })
            .Where(x => x.Pass == false)
            .Select(x => x.Message)
            .Join(Environment.NewLine);

        if (!negativeTestResults.IsEmpty())
        {
            string msg = $"Batch id={batch.Id} has errors" + Environment.NewLine + negativeTestResults;
            _logger.Error(msg);
            throw new ArgumentException(msg);
        }

        var groups = batch.Items.GroupBy(x => getToId(x));

        foreach (var groupItem in groups)
        {
            DocumentId bankId = (DocumentId)groupItem.Key;
            string bankName = bankId.GetBankName();
            QueueClient<QueueMessage> client = await GetClient(bankName, token);

            var trxBatch = new TrxBatch<T>
            {
                Items = new List<T>(groupItem)
            };

            QueueMessage queueMessage = trxBatch.ToQueueMessage();
            await client.Send(queueMessage);

            _logger.Information($"Sent batch to bank={bankName}, count={trxBatch.Items.Count}");
        }
    }

    public async Task<QueueOption> GetQueueOption(CancellationToken token)
    {
        _banks.TryGetValue(_clearingOption.BankName, out BankDetail? bankDetail)
            .VerifyAssert(x => x == true, $"Bank {_clearingOption.BankName} not found");

        return await GetQueueOption((DocumentId)bankDetail!.QueueId, token);
    }

    private async Task<QueueOption> GetQueueOption(DocumentId documentId, CancellationToken token)
    {
        DirectoryEntry queueEntry = (await _directoryClient.Get(documentId, token))
            .VerifyNotNull($"{documentId} does not exist");

        return queueEntry.Properties
            .ToConfiguration()
            .Bind<QueueOption>();
    }

    private async Task LoadDirectory(CancellationToken token)
    {
        await _asyncLock.WaitAsync();

        try
        {
            if (_banks.Count > 0) return;

            DirectoryEntry entry = (await _directoryClient.Get(_clearingOption.BankDirectoryId))
                .VerifyNotNull($"Bank directory {_clearingOption.BankDirectoryId} does not exist");

            foreach (var bank in entry.Properties.Select(x => x.ToKeyValuePair()))
            {
                DocumentId bankId = (DocumentId)bank.Value;
                DirectoryEntry bankEntry = (await _directoryClient.Get(bankId)).VerifyNotNull($"BankId={bankId.Path} does not exist");

                string queueId = bankEntry.Properties.GetValue(PropertyName.QueueId).VerifyNotEmpty($"{bankId} does not have property {PropertyName.QueueId}=...");

                _banks.Add(bank.Key, new BankDetail { BankId = bankId, QueueId = queueId });
            }
        }
        finally
        {
            _asyncLock.Release();
        }
    }

    private record BankDetail
    {
        public DocumentId BankId { get; init; } = null!;

        public string QueueId { get; init; } = null!;
    }
}
