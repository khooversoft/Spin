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
        subject.VerifyNotNull(nameof(subject));

        subject.BankName.VerifyNotEmpty($"{nameof(subject.BankName)} is required");
        subject.HostUrl.VerifyNotEmpty($"{nameof(subject.HostUrl)} is required");
        subject.ApiKey.VerifyNotEmpty($"{nameof(subject.ApiKey)} is required");
        subject.QueueId.VerifyNotEmpty($"{nameof(subject.QueueId)} is required");
        subject.Container.VerifyNotEmpty($"{nameof(subject.Container)} is required");

        return subject;
    }
}