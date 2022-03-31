using Directory.sdk.Client;
using Directory.sdk.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Application;
using Toolbox.Document;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace Directory.sdk.Model;

public record BankDirectoryRecord
{
    public IReadOnlyDictionary<string, BankDirectoryEntry> Banks { get; set; } = null!;
}

public record BankDirectoryEntry
{
    public string BankName { get; set; } = null!;

    public string DirectoryId { get; set; } = null!;
}



public static class BankDirectoryRecordExtensions
{
    public static BankDirectoryRecord Verify(this BankDirectoryRecord subject)
    {
        subject.VerifyNotNull(nameof(subject));

        subject.Banks.VerifyNotNull(nameof(subject.Banks));
        subject.Banks.VerifyAssert(x => x?.Count > 0, "Banks list is empty");

        subject.Banks
            .ForEach(x =>
            {
                x.Value.Verify();
                x.Key.VerifyAssert(y => x.Key == x.Value.BankName, "key != BankName");
            });

        return subject;
    }

    public static BankDirectoryEntry Verify(this BankDirectoryEntry subject)
    {
        subject.VerifyNotNull(nameof(subject));

        subject.BankName.VerifyNotEmpty($"{nameof(subject.BankName)} is required");
        subject.DirectoryId.VerifyNotEmpty($"{nameof(subject.DirectoryId)} is required");

        return subject;
    }

    public static async Task<BankDirectoryRecord> GetBankDirectory(this DirectoryClient client, RunEnvironment runEnvironment)
    {
        client.VerifyNotNull(nameof(client));

        var documentId = (DocumentId)$"{runEnvironment}/setting/BankDirectory";

        DirectoryEntry entry = (await client.Get(documentId))
            .VerifyNotNull($"Configuration {documentId} not found");

        return new BankDirectoryRecord
        {
            Banks = entry.Properties
                .Select(x => x.ToKeyValuePair())
                .ToDictionary(x => x.Key, x => new BankDirectoryEntry { BankName = x.Key, DirectoryId = x.Value })
        }.Verify();
    }
}

