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
        subject.NotNull(nameof(subject));

        subject.BankName.NotEmpty($"{nameof(subject.BankName)} is required");
        subject.HostUrl.NotEmpty($"{nameof(subject.HostUrl)} is required");
        subject.ApiKey.NotEmpty($"{nameof(subject.ApiKey)} is required");
        subject.QueueId.NotEmpty($"{nameof(subject.QueueId)} is required");
        subject.Container.NotEmpty($"{nameof(subject.Container)} is required");

        return subject;
    }
}