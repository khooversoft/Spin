using Toolbox.Abstractions;
using Toolbox.Block;
using Toolbox.Tools;

namespace Contract.sdk.Models;

public record BlkHeader
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
        DocumentId.VerifyId(subject.DocumentId);
        subject.Creator.NotEmpty();
        subject.Description.NotEmpty();
    }
}
