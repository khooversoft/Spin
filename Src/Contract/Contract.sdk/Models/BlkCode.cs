using Toolbox.Tools;

namespace Contract.sdk.Models;

public record BlkCode : BlkBase
{
    public string Language { get; init; } = "C#";

    public string Framework { get; init; } = ".net6.0";

    public IReadOnlyList<string> Lines { get; init; } = new List<string>();
}

public static class BlkCodeExtensions
{
    public static void Verify(this BlkCode blkCode)
    {
        blkCode.VerifyBase();
        blkCode.Language.VerifyNotEmpty(nameof(blkCode.Language));
        blkCode.Framework.VerifyNotEmpty(nameof(blkCode.Framework));
        blkCode.Lines.VerifyNotNull(nameof(blkCode.Lines));
    }
}
