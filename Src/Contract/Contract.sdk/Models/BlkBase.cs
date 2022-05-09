using Toolbox.Tools;

namespace Contract.sdk.Models;

public record BlkBase
{
    public DateTimeOffset Date { get; init; } = DateTimeOffset.UtcNow;

    public string PrincipleId { get; init; } = null!;

    public IReadOnlyList<string>? Properties { get; init; }
}


public static class BlkBaseExtensions
{
    public static void VerifyBase(this BlkBase subject)
    {
        subject.NotNull(nameof(subject));
        subject.PrincipleId.NotEmpty(nameof(subject.PrincipleId));
    }
}
