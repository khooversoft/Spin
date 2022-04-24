using Toolbox.Abstractions;
using Toolbox.Tools;

namespace Contract.sdk.Models;

public record BlkHeader : BlkBase
{
    public string DocumentId { get; init; } = null!;

    public string Creator { get; init; } = null!;

    public string Description { get; init; } = null!;

    public DateTime Created { get; init; } = DateTime.UtcNow;
}


public static class BlkHeaderExtensions
{
    public static void Verify(this BlkHeader subject)
    {
        subject.VerifyNotNull(nameof(subject));
        subject.VerifyBase();

        DocumentId.VerifyId(subject.DocumentId);
        subject.Creator.VerifyNotEmpty(nameof(subject.Creator));
        subject.Description.VerifyNotEmpty(nameof(subject.Description));
    }
}
