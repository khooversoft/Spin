using Directory.sdk.Client;
using Directory.sdk.Service;
using System.Threading.Tasks;
using Toolbox.Application;
using Toolbox.Configuration;
using Toolbox.Document;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace Directory.sdk.Model;

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
}
