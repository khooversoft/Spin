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

public class BankServiceRecord
{
    public string HostUrl { get; set; } = null!;

    public string ApiKey { get; set; } = null!;

    public string QueueId { get; set; } = null!;

    public string Container { get; set; } = null!;
}

public static class BankServiceRecordExtensions
{
    public static BankServiceRecord Verify(this BankServiceRecord subject)
    {
        subject.VerifyNotNull(nameof(subject));

        subject.HostUrl.VerifyNotEmpty($"{nameof(subject.HostUrl)} is required");
        subject.ApiKey.VerifyNotEmpty($"{nameof(subject.ApiKey)} is required");
        subject.QueueId.VerifyNotEmpty($"{nameof(subject.QueueId)} is required");
        subject.Container.VerifyNotEmpty($"{nameof(subject.Container)} is required");

        return subject;
    }

    public static async Task<BankServiceRecord> GetBankServiceRecord(this DirectoryClient client, RunEnvironment runEnvironment, string bankName)
    {
        client.VerifyNotNull(nameof(client));
        bankName.VerifyNotNull(nameof(bankName));

        var documentId = (DocumentId)$"{runEnvironment}/service/{bankName}";

        DirectoryEntry entry = (await client.Get(documentId))
            .VerifyNotNull($"Configuration {documentId} not found");

        return entry
            .ConvertTo<BankServiceRecord>()
            .Verify();
    }
}