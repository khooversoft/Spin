using Toolbox.Tools;

namespace Directory.sdk.Model;

public record BankServiceRecord
{
    public string BankName { get; init; } = null!;

    public string HostUrl { get; init; } = null!;

    public string ApiKey { get; init; } = null!;

    public string QueueId { get; init; } = null!;

    public string Container { get; init; } = null!;
}

public static class BankServiceRecordExtensions
{
    public static BankServiceRecord Verify(this BankServiceRecord subject)
    {
        subject.NotNull();

        subject.BankName.NotEmpty(name: $"{nameof(subject.BankName)} is required");
        subject.HostUrl.NotEmpty(name: $"{nameof(subject.HostUrl)} is required");
        subject.ApiKey.NotEmpty(name: $"{nameof(subject.ApiKey)} is required");
        subject.QueueId.NotEmpty(name: $"{nameof(subject.QueueId)} is required");
        subject.Container.NotEmpty(name: $"{nameof(subject.Container)} is required");

        return subject;
    }
}