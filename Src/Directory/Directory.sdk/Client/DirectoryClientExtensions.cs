using Directory.sdk.Model;
using Directory.sdk.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Toolbox.Application;
using Toolbox.Azure.Queue;
using Toolbox.Document;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace Directory.sdk.Client;

public static class DirectoryClientExtensions
{
    public static async Task<IReadOnlyList<StorageRecord>> GetStorageRecord(this DirectoryClient client, RunEnvironment runEnvironment, string settingName, CancellationToken token = default)
    {
        client.VerifyNotNull(nameof(client));

        var documentId = (DocumentId)$"{runEnvironment}/setting/{settingName.GetInstanceName()}";

        DirectoryEntry entry = (await client.Get(documentId, token))
            .VerifyNotNull($"Configuration {documentId} not found");

        List<StorageRecord> list = new();

        foreach (string item in entry.Properties)
        {
            DirectoryEntry storage = (await client.Get((DocumentId)item, token))
                .VerifyNotNull($"Configuration {item} not found");

            StorageRecord storageRecord = storage.Properties
                .ToConfiguration()
                .Bind<StorageRecord>()
                .VerifyNotNull($"Cannot bind to {nameof(StorageRecord)}")
                .Verify();

            list.Add(storageRecord);
        }

        return list;
    }

    public static async Task<ServiceRecord> GetServiceRecord(this DirectoryClient client, RunEnvironment runEnvironment, string serviceName, CancellationToken token = default)
    {
        client.VerifyNotNull(nameof(client));

        var documentId = (DocumentId)$"{runEnvironment}/service/{serviceName.GetInstanceName()}";

        DirectoryEntry entry = (await client.Get(documentId, token))
            .VerifyNotNull($"Configuration {documentId} not found");

        return entry
            .ConvertTo<ServiceRecord>()
            .Verify();
    }

    public static async Task<QueueOption> GetQueueOption(this DirectoryClient client, RunEnvironment runEnvironment, string queueName, CancellationToken token = default)
    {
        client.VerifyNotNull(nameof(client));

        var documentId = (DocumentId)$"{runEnvironment}/queue/{queueName.GetInstanceName()}";

        DirectoryEntry queueEntry = (await client.Get(documentId, token))
            .VerifyNotNull($"{documentId} does not exist");

        return queueEntry.Properties
            .ToConfiguration()
            .Bind<QueueOption>();
    }

    public static async Task<BankServiceRecord> GetBankServiceRecord(this DirectoryClient client, RunEnvironment runEnvironment, string bankName, CancellationToken token = default)
    {
        client.VerifyNotNull(nameof(client));
        bankName.VerifyNotNull(nameof(bankName));

        var documentId = (DocumentId)$"{runEnvironment}/service/{bankName.GetInstanceName()}";

        DirectoryEntry entry = (await client.Get(documentId, token))
            .VerifyNotNull($"Configuration {documentId} not found");

        return entry
            .ConvertTo<BankServiceRecord>()
            .Func(x => x with { BankName = bankName })
            .Verify();
    }

    public static async Task<BankDirectoryRecord> GetBankDirectory(this DirectoryClient client, RunEnvironment runEnvironment, CancellationToken token = default)
    {
        client.VerifyNotNull(nameof(client));

        var documentId = (DocumentId)$"{runEnvironment}/setting/BankDirectory";

        DirectoryEntry entry = (await client.Get(documentId, token))
            .VerifyNotNull($"Configuration {documentId} not found");

        return new BankDirectoryRecord
        {
            Banks = entry.Properties
                .Select(x => x.ToKeyValuePair())
                .ToDictionary(x => x.Key, x => new BankDirectoryEntry { BankName = x.Key, DirectoryId = x.Value })
        }.Verify();
    }

    public static async Task<IReadOnlyList<BankServiceRecord>> GetBankServiceRecords(this DirectoryClient client, RunEnvironment runEnvironment, CancellationToken token = default)
    {
        client.VerifyNotNull(nameof(client));

        BankDirectoryRecord bankDirectoryRecord = await client.GetBankDirectory(runEnvironment, token);

        var list = new List<BankServiceRecord>();
        foreach (BankDirectoryEntry bank in bankDirectoryRecord.Banks.Values)
        {
            BankServiceRecord bankEntry = (await client.GetBankServiceRecord(runEnvironment, bank.BankName, token))
                .VerifyNotNull($"BankId={bank.DirectoryId} does not exist");

            list.Add(bankEntry);
        }

        return list;
    }

    private static string GetInstanceName(this string name)
    {
        name.VerifyNotEmpty(nameof(name));

        string[] item = name.Split('/');

        if (item.Length == 1) return item[0];

        return item
            .Skip(2)
            .FirstOrDefault() ?? throw new ArgumentException("Directory instance name is required (e.g. '{Environment}/{type}/{instance}'), name=" + name);
    }
}
