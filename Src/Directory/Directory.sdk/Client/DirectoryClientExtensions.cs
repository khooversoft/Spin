using Directory.sdk.Model;
using Directory.sdk.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Toolbox.Abstractions;
using Toolbox.Abstractions.Application;
using Toolbox.Abstractions.Protocol;
using Toolbox.Abstractions.Tools;
using Toolbox.Azure.Queue;
using Toolbox.DocumentStore;
using Toolbox.Extensions;

namespace Directory.sdk.Client;

public static class DirectoryClientExtensions
{
    public static async Task<IReadOnlyList<StorageRecord>> GetStorageRecord(this DirectoryClient client, RunEnvironment runEnvironment, string settingName, CancellationToken token = default)
    {
        client.NotNull();

        var documentId = (DocumentId)$"{runEnvironment}/setting/{settingName.GetInstanceName()}";

        DirectoryEntry entry = (await client.Get(documentId, token))
            .NotNull(name: $"Configuration {documentId} not found");

        List<StorageRecord> list = new();

        foreach (string item in entry.Properties)
        {
            DirectoryEntry storage = (await client.Get((DocumentId)item, token))
                .NotNull(name: $"Configuration {item} not found");

            StorageRecord storageRecord = storage.Properties
                .ToConfiguration()
                .Bind<StorageRecord>()
                .NotNull(name: $"Cannot bind to {nameof(StorageRecord)}")
                .Verify();

            list.Add(storageRecord);
        }

        return list;
    }

    public static async Task<ServiceRecord> GetServiceRecord(this DirectoryClient client, RunEnvironment runEnvironment, string serviceName, CancellationToken token = default)
    {
        client.NotNull();

        var documentId = (DocumentId)$"{runEnvironment}/service/{serviceName.GetInstanceName()}";

        DirectoryEntry entry = (await client.Get(documentId, token))
            .NotNull(name: $"Configuration {documentId} not found");

        return entry
            .ConvertTo<ServiceRecord>()
            .Verify();
    }

    public static async Task<QueueOption> GetQueueOption(this DirectoryClient client, RunEnvironment runEnvironment, string queueName, CancellationToken token = default)
    {
        client.NotNull();

        var documentId = (DocumentId)$"{runEnvironment}/queue/{queueName.GetInstanceName()}";

        DirectoryEntry queueEntry = (await client.Get(documentId, token))
            .NotNull(name: $"{documentId} does not exist");

        return queueEntry.Properties
            .ToConfiguration()
            .Bind<QueueOption>();
    }

    public static async Task<BankServiceRecord> GetBankServiceRecord(this DirectoryClient client, RunEnvironment runEnvironment, string bankName, CancellationToken token = default)
    {
        client.NotNull();
        bankName.NotNull();

        var documentId = (DocumentId)$"{runEnvironment}/service/{bankName.GetInstanceName()}";

        DirectoryEntry entry = (await client.Get(documentId, token))
            .NotNull(name: $"Configuration {documentId} not found");

        return entry
            .ConvertTo<BankServiceRecord>()
            .Func(x => x with { BankName = bankName })
            .Verify();
    }

    public static async Task<BankDirectoryRecord> GetBankDirectory(this DirectoryClient client, RunEnvironment runEnvironment, CancellationToken token = default)
    {
        client.NotNull();

        var documentId = (DocumentId)$"{runEnvironment}/setting/BankDirectory";

        DirectoryEntry entry = (await client.Get(documentId, token))
            .NotNull(name: $"Configuration {documentId} not found");

        return new BankDirectoryRecord
        {
            Banks = entry.Properties
                .Select(x => x.ToKeyValuePair())
                .ToDictionary(x => x.Key, x => new BankDirectoryEntry { BankName = x.Key, DirectoryId = x.Value })
        }.Verify();
    }

    public static async Task<IReadOnlyList<BankServiceRecord>> GetBankServiceRecords(this DirectoryClient client, RunEnvironment runEnvironment, CancellationToken token = default)
    {
        client.NotNull();

        BankDirectoryRecord bankDirectoryRecord = await client.GetBankDirectory(runEnvironment, token);

        var list = new List<BankServiceRecord>();
        foreach (BankDirectoryEntry bank in bankDirectoryRecord.Banks.Values)
        {
            BankServiceRecord bankEntry = (await client.GetBankServiceRecord(runEnvironment, bank.BankName, token))
                .NotNull(name: $"BankId={bank.DirectoryId} does not exist");

            list.Add(bankEntry);
        }

        return list;
    }

    private static string GetInstanceName(this string name)
    {
        name.NotEmpty();

        string[] item = name.Split('/');

        if (item.Length == 1) return item[0];

        return item
            .Skip(2)
            .FirstOrDefault() ?? throw new ArgumentException("Directory instance name is required (e.g. '{Environment}/{type}/{instance}'), name=" + name);
    }
}
