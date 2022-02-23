using Directory.sdk.Client;
using Directory.sdk.Service;
using System.Threading.Tasks;
using Toolbox.Application;
using Toolbox.Configuration;
using Toolbox.Document;
using Toolbox.Extensions;
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

            var result = entry.ConvertToServiceRecord();
            return result;
        }

        public static ServiceRecord ConvertToServiceRecord(this DirectoryEntry entry)
        {
            entry.VerifyNotNull(nameof(entry));

            return entry.Properties
                .ToConfiguration()
                .Bind<ServiceRecord>()
                .VerifyNotNull($"Cannot bind to {nameof(ServiceRecord)}")
                .Verify();
        }
    }
}