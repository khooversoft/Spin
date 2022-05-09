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
        subject.NotNull(nameof(subject));

        subject.Container.NotEmpty($"{nameof(subject.Container)} is required");
        subject.AccountName.NotEmpty($"{nameof(subject.AccountName)} is required");
        subject.ContainerName.NotEmpty($"{nameof(subject.ContainerName)} is required");
        subject.AccountKey.NotEmpty($"{nameof(subject.AccountKey)} is required");
        subject.BasePath.NotEmpty($"{nameof(subject.BasePath)} is required");

        return subject;
    }

}
