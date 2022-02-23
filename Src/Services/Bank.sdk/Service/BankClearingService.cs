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
using Toolbox.Application;
using Toolbox.Azure.Queue;
using Toolbox.Document;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace Bank.sdk.Service;

public class BankClearingService
{
    private readonly ClearingOption _clearingOption;
    private readonly DirectoryClient _directoryClient;
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger<BankClearingService> _logger;
    private readonly Dictionary<string, BankDetail> _banks = new(StringComparer.OrdinalIgnoreCase);
    private readonly SemaphoreSlim _asyncLock = new SemaphoreSlim(1, 1);

    private int _lock = 0;
    private QueueReceiver<QueueMessage>? _receiver;

    public BankClearingService(ClearingOption clearingOption, DirectoryClient directoryClient, ILoggerFactory loggerFactory)
    {
        _clearingOption = clearingOption.Verify();
        _directoryClient = directoryClient;
        _loggerFactory = loggerFactory;
        _logger = _loggerFactory.CreateLogger<BankClearingService>();
    }

    public async Task Start(Func<QueueMessage, Task<bool>> receiver, CancellationToken token)
    {
        receiver.VerifyNotNull(nameof(receiver));

        int lockState = Interlocked.CompareExchange(ref _lock, 1, 0);
        if (lockState == 1) return;

        try
        {
            if (_receiver != null) return;

            await LoadDirectory(token);

            _banks.TryGetValue(_clearingOption.BankName, out BankDetail? bankDetail)
                .VerifyAssert(x => x == true, $"Bank {_clearingOption.BankName} not found");

            QueueOption queueOption = await GetQueueOption((DocumentId)bankDetail!.BankId, token);

            var receiverOption = new QueueReceiverOption<QueueMessage>
            {
                QueueOption = queueOption,
                Receiver = receiver
            };

            _receiver = new QueueReceiver<QueueMessage>(receiverOption, _loggerFactory.CreateLogger<QueueReceiver<QueueMessage>>());
            _receiver.Start();
        }
        finally
        {
            _lock = 0;
        }
    }

    public async Task Stop()
    {
        var receiver = Interlocked.Exchange(ref _receiver, null);
        if (receiver != null) return;

        await _receiver!.Stop();
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

    private async Task<QueueOption> GetQueueOption(DocumentId documentId, CancellationToken token)
    {
        DirectoryEntry queueEntry = (await _directoryClient.Get(documentId, token))
            .VerifyNotNull($"{documentId} does not exist");

        return queueEntry.Properties
            .ToConfiguration()
            .Bind<QueueOption>();
    }

    private record BankDetail
    {
        public DocumentId BankId { get; init; } = null!;

        public string QueueId { get; init; } = null!;
    }
}
