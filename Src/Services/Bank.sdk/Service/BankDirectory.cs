using Bank.sdk.Model;
using Directory.sdk;
using Directory.sdk.Client;
using Directory.sdk.Service;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
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
    private readonly ConcurrentDictionary<string, BankDetail> _banks = new ConcurrentDictionary<string, BankDetail>(StringComparer.OrdinalIgnoreCase);
    private readonly ClearingOption _clearingOption;
    private readonly ConcurrentDictionary<string, QueueClient<QueueMessage>> _clientCache = new ConcurrentDictionary<string, QueueClient<QueueMessage>>(StringComparer.OrdinalIgnoreCase);

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

        if (_clientCache.TryGetValue(bankName, out QueueClient<QueueMessage>? cacheClient)) return cacheClient;

        _banks.TryGetValue(bankName, out BankDetail? bankDetail)
            .VerifyAssert(x => x == true, $"No {bankName} was found");

        QueueOption option = await GetQueueOption((DocumentId)bankDetail!.QueueId, token);

        return _clientCache.GetOrAdd(bankName, _ => new QueueClient<QueueMessage>(option, _loggerFactory.CreateLogger<QueueClient<QueueMessage>>()));
    }

    public bool IsBankNameExist(string bankName) => _banks.ContainsKey(bankName);

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

            _logger.LogTrace("Loading directory");

            DirectoryEntry entry = (await _directoryClient.Get(_clearingOption.BankDirectoryId))
                .VerifyNotNull($"Bank directory {_clearingOption.BankDirectoryId} does not exist");

            foreach (var bank in entry.Properties.Select(x => x.ToKeyValuePair()))
            {
                DocumentId bankId = (DocumentId)bank.Value;
                DirectoryEntry bankEntry = (await _directoryClient.Get(bankId)).VerifyNotNull($"BankId={bankId.Path} does not exist");

                string queueId = bankEntry.Properties.GetValue(PropertyName.QueueId).VerifyNotEmpty($"{bankId} does not have property {PropertyName.QueueId}=...");

                _banks[bank.Key] = new BankDetail { BankId = bankId, QueueId = queueId };
            }

            _logger.LogTrace("Loaded directory, count={count}", _banks.Count);
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
