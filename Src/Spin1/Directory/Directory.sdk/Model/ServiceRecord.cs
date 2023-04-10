using Toolbox.Abstractions.Tools;

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
        subject.NotNull();

        subject.HostUrl.NotEmpty(name: $"{nameof(subject.HostUrl)} is required");
        subject.ApiKey.NotEmpty(name: $"{nameof(subject.ApiKey)} is required");

        return subject;
    }
}
