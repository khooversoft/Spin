using Directory.sdk.Client;
using Directory.sdk.Model;
using Directory.sdk.Service;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using Toolbox.Abstractions;
using Toolbox.Azure.Queue;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace Bank.sdk.Service;

public class BankDirectory
{
    private readonly SemaphoreSlim _asyncLock = new SemaphoreSlim(1, 1);
    private readonly DirectoryClient _directoryClient;
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger<BankDirectory> _logger;
    private readonly ConcurrentDictionary<string, BankServiceRecord> _banks = new ConcurrentDictionary<string, BankServiceRecord>(StringComparer.OrdinalIgnoreCase);
    private readonly BankOption _bankOption;
    private readonly ConcurrentDictionary<string, QueueClient<QueueMessage>> _clientCache = new ConcurrentDictionary<string, QueueClient<QueueMessage>>(StringComparer.OrdinalIgnoreCase);

    internal BankDirectory(BankOption bankOption, DirectoryClient directoryClient, ILoggerFactory loggerFactory)
    {
        _bankOption = bankOption;
        _directoryClient = directoryClient;
        _loggerFactory = loggerFactory;
        _logger = loggerFactory.CreateLogger<BankDirectory>();
    }

    public async Task<QueueClient<QueueMessage>> GetClient(string bankName, CancellationToken token)
    {
        bankName.NotEmpty(nameof(bankName));

        await LoadDirectory(token);

        if (_clientCache.TryGetValue(bankName, out QueueClient<QueueMessage>? cacheClient)) return cacheClient;

        _banks.TryGetValue(bankName, out BankServiceRecord? bankDetail)
            .Assert(x => x == true, $"No {bankName} was found");

        QueueOption option = await GetQueueOption((DocumentId)bankDetail!.QueueId, token);

        return _clientCache.GetOrAdd(bankName, _ => new QueueClient<QueueMessage>(option, _loggerFactory.CreateLogger<QueueClient<QueueMessage>>()));
    }

    public bool IsBankNameExist(string bankName) => _banks.ContainsKey(bankName);

    public async Task<QueueOption> GetQueueOption(CancellationToken token = default)
    {
        await LoadDirectory(token);

        _banks.TryGetValue(_bankOption.BankName, out BankServiceRecord? bankDetail)
            .Assert(x => x == true, $"Bank {_bankOption.BankName} not found");

        return await GetQueueOption((DocumentId)bankDetail!.QueueId, token);
    }

    private async Task<QueueOption> GetQueueOption(DocumentId documentId, CancellationToken token)
    {
        DirectoryEntry queueEntry = (await _directoryClient.Get(documentId, token))
            .NotNull($"{documentId} does not exist");

        return queueEntry.Properties
            .ToConfiguration()
            .Bind<QueueOption>();
    }

    public async Task LoadDirectory(CancellationToken token = default)
    {
        await _asyncLock.WaitAsync();

        try
        {
            if (_banks.Count > 0) return;

            _logger.LogTrace("Loading directory");

            IReadOnlyList<BankServiceRecord> banks = await _directoryClient.GetBankServiceRecords(_bankOption.RunEnvironment);
            banks.ForEach(x => _banks[x.BankName] = x);

            _logger.LogTrace("Loaded directory, count={count}", _banks.Count);
        }
        finally
        {
            _asyncLock.Release();
        }
    }
}
