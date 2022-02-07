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

namespace Directory.sdk.Model
{
    public record StorageRecord
    {
        public string Container { get; init; } = null!;

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

            subject.Container.VerifyNotEmpty($"{nameof(subject.Container)} is required");
            subject.AccountName.VerifyNotEmpty($"{nameof(subject.AccountName)} is required");
            subject.ContainerName.VerifyNotEmpty($"{nameof(subject.ContainerName)} is required");
            subject.AccountKey.VerifyNotEmpty($"{nameof(subject.AccountKey)} is required");
            subject.BasePath.VerifyNotEmpty($"{nameof(subject.BasePath)} is required");

            return subject;
        }

        public static async Task<IReadOnlyList<StorageRecord>> GetStorageRecord(this DirectoryClient client, RunEnvironment runEnvironment, string settingName)
        {
            client.VerifyNotNull(nameof(client));
            settingName.VerifyNotNull(nameof(settingName));

            var documentId = (DocumentId)$"{runEnvironment}/setting/{settingName}";

            DirectoryEntry entry = (await client.Get(documentId))
                .VerifyNotNull($"Configuration {documentId} not found");

            List<StorageRecord> list = new();

            foreach(EntryProperty property in entry.Properties.Values)
            {
                DirectoryEntry storage = (await client.Get((DocumentId)property.Value))
                    .VerifyNotNull($"Configuration {property} not found");

                list.Add(storage.ConvertToStorageRecord());
            }

            return list;
        }

        public static StorageRecord ConvertToStorageRecord(this DirectoryEntry entry)
        {
            entry.VerifyNotNull(nameof(entry));

            return new StorageRecord
            {
                Container = entry.GetPropertyValue(nameof(StorageRecord.Container))!,
                AccountName = entry.GetPropertyValue(nameof(StorageRecord.AccountName))!,
                ContainerName = entry.GetPropertyValue(nameof(StorageRecord.ContainerName))!,
                AccountKey = entry.GetPropertyValue(nameof(StorageRecord.AccountKey))!,
                BasePath = entry.GetPropertyValue(nameof(StorageRecord.BasePath))!,
            }.Verify();
        }
    }
}
