using Toolbox.Abstractions;
using Toolbox.Block;
using Toolbox.Tools;

namespace Contract.sdk.Models;

public record BlkHeader : BlkBase
{
    public string DocumentId { get; init; } = null!;

    public string Creator { get; init; } = null!;

    public string Description { get; init; } = null!;

    public IReadOnlyList<DataBlock>? Blocks { get; init; }
}


public static class BlkHeaderExtensions
{
    public static void Verify(this BlkHeader subject)
    {
        subject.NotNull(nameof(subject));
        subject.VerifyBase();

        DocumentId.VerifyId(subject.DocumentId);
        subject.Creator.NotEmpty(nameof(subject.Creator));
        subject.Description.NotEmpty(nameof(subject.Description));
    }
}
