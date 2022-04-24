using Toolbox.Tools;

namespace Contract.sdk.Models;

public record BlkBase
{
    public DateTime BlockDate { get; init; } = DateTime.UtcNow;

    public string PrincipalId { get; init; } = null!;

    public IReadOnlyDictionary<string, string> Properties { get; init; } = new Dictionary<string, string>();
}


public static class BlkBaseExtensions
{
    public static void VerifyBase(this BlkBase subject)
    {
        subject.VerifyNotNull(nameof(subject));
        subject.PrincipalId.VerifyNotEmpty(nameof(subject.PrincipalId));
        subject.Properties.VerifyNotNull(nameof(subject.Properties));
    }
}
