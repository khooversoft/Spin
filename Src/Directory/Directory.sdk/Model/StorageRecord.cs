using Toolbox.Tools;

namespace Directory.sdk.Model;

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

}
