using Toolbox.Tools;

namespace Directory.sdk.Model;

public record ValidateRequest
{
    public string DirectoryId { get; init; } = null!;

    public string ClassType { get; init; } = ClassTypeName.Identity;

    public string Jwt { get; init; } = null!;
}


public static class ValidateRequestExtensions
{
    public static void Verify(this ValidateRequest subject)
    {
        subject.VerifyNotNull(nameof(subject));
        subject.DirectoryId.VerifyNotEmpty(nameof(subject.DirectoryId));
        subject.ClassType.VerifyNotEmpty(nameof(subject.ClassType));
        subject.Jwt.VerifyNotEmpty(nameof(subject.Jwt));
    }
}
