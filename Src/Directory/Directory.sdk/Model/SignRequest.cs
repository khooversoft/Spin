using Toolbox.Tools;

namespace Directory.sdk.Model;

public record SignRequest
{
    public string DirectoryId { get; init; } = null!;

    public string ClassType { get; init; } = ClassTypeName.Identity;

    public string Digest { get; init; } = null!;
}


public static class SignRequestExtensions
{
    public static void Verify(this SignRequest subject)
    {
        subject.VerifyNotNull(nameof(subject));
        subject.DirectoryId.VerifyNotEmpty(nameof(subject.DirectoryId));
        subject.ClassType.VerifyNotEmpty(nameof(subject.ClassType));
        subject.Digest.VerifyNotEmpty(nameof(subject.Digest));
    }
}
