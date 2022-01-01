using Directory.sdk.Client;
using Directory.sdk.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Toolbox.Application;
using Toolbox.Document;
using Toolbox.Tools;

namespace Directory.sdk.Model
{
    public record ServiceRecord
    {
        public string HostUrl { get; init; } = null!;

        public string ApiKey { get; init; } = null!;
    }

    public static class ServiceRecordExtensions
    {
        public static ServiceRecord Verify(this ServiceRecord subject)
        {
            subject.VerifyNotNull(nameof(subject));

            subject.HostUrl.VerifyNotEmpty($"{nameof(subject.HostUrl)} is required");
            subject.ApiKey.VerifyNotEmpty($"{nameof(subject.ApiKey)} is required");

            return subject;
        }

        public static async Task<ServiceRecord> GetServiceRecord(this DirectoryClient client, RunEnvironment runEnvironment, string serviceName)
        {
            client.VerifyNotNull(nameof(client));
            serviceName.VerifyNotNull(nameof(serviceName));

            var documentId = (DocumentId)$"{runEnvironment}/service/{serviceName}";

            DirectoryEntry entry = (await client.Get(documentId))
                .VerifyNotNull($"Configuration {documentId} not found");

            return entry.ConvertToServiceRecord();
        }

        public static ServiceRecord ConvertToServiceRecord(this DirectoryEntry entry)
        {
            entry.VerifyNotNull(nameof(entry));

            return new ServiceRecord
            {
                HostUrl = entry.GetPropertyValue(nameof(ServiceRecord.HostUrl))!,
                ApiKey = entry.GetPropertyValue(nameof(ServiceRecord.ApiKey))!,
            }.Verify();
        }
    }
}