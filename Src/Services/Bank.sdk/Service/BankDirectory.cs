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
using Toolbox.Tools;

namespace Bank.sdk.Service;

public class BankDirectory
{
    private readonly SemaphoreSlim _asyncLock = new SemaphoreSlim(1, 1);
    private readonly DirectoryClient _directoryClient;
    private readonly ILoggerFactory _loggerFactory;
    private readonly Dictionary<string, BankDetail> _banks = new(StringComparer.OrdinalIgnoreCase);
    private readonly ClearingOption _clearingOption;

    public BankDirectory(ClearingOption clearingOption, DirectoryClient directoryClient, ILoggerFactory loggerFactory)
    {
        _clearingOption = clearingOption;
        _directoryClient = directoryClient;
        _loggerFactory = loggerFactory;
    }

    public async Task<QueueClient<QueueMessage>> GetClient(string bankName, CancellationToken token)
    {
        bankName.VerifyNotEmpty(nameof(bankName));

        await LoadDirectory(token);

        if (!_banks.TryGetValue(bankName, out BankDetail? bankDetail)) throw new ArgumentException($"No {bankName} was found");

        QueueOption option = await GetQueueOption((DocumentId)bankDetail.QueueId, token);

        return new QueueClientBuilder<QueueMessage>()
            .SetQueueOption(option)
            .SetLoggerFactory(_loggerFactory)
            .Build();
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
