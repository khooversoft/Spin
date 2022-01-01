using Directory.sdk.Client;
using Directory.sdk.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Application;
using Toolbox.Document;
using Toolbox.Tools;

namespace Directory.sdk.Model
{
    public record StorageRecord
    {
        public string AccountName { get; init; } = null!;

        public string ContainerName { get; init; } = null!;

        public string AccountKey { get; init; } = null!;

        public string BasePath { get; init; } = null!;
    }

    public static class StorageRecordExtensions
    {
        public static StorageRecord Verify(this StorageRecord subject)
        {
            subject.VerifyNotNull(nameof(subject));

            subject.AccountName.VerifyNotEmpty($"{nameof(subject.AccountName)} is required");
            subject.ContainerName.VerifyNotEmpty($"{nameof(subject.ContainerName)} is required");
            subject.AccountKey.VerifyNotEmpty($"{nameof(subject.AccountKey)} is required");
            subject.BasePath.VerifyNotEmpty($"{nameof(subject.BasePath)} is required");

            return subject;
        }

        public static async Task<StorageRecord> GetStorageRecord(this DirectoryClient client, RunEnvironment runEnvironment, string settingName)
        {
            client.VerifyNotNull(nameof(client));
            settingName.VerifyNotNull(nameof(settingName));

            var documentId = (DocumentId)$"{runEnvironment}/service/{settingName}";

            DirectoryEntry entry = (await client.Get(documentId))
                .VerifyNotNull($"Configuration {documentId} not found");

            return entry.ConvertToStorageRecord();
        }

        public static StorageRecord ConvertToStorageRecord(this DirectoryEntry entry)
        {
            entry.VerifyNotNull(nameof(entry));

            return new StorageRecord
            {
                AccountName = entry.GetPropertyValue(nameof(StorageRecord.AccountName))!,
                ContainerName = entry.GetPropertyValue(nameof(StorageRecord.ContainerName))!,
                AccountKey = entry.GetPropertyValue(nameof(StorageRecord.AccountKey))!,
                BasePath = entry.GetPropertyValue(nameof(StorageRecord.AccountKey))!,
            }.Verify();
        }
    }
}
