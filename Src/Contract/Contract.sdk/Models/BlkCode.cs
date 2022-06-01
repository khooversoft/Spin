using Toolbox.Tools;

namespace Contract.sdk.Models;

public record BlkCode : BlkBase
{
    public string Language { get; init; } = "C#";

    public string Framework { get; init; } = ".net6.0";

    public IReadOnlyList<string> Lines { get; init; } = Array.Empty<string>();
}

public static class BlkCodeExtensions
{
    public static void Verify(this BlkCode blkCode)
    {
        blkCode.VerifyBase();
        blkCode.Language.NotEmpty();
        blkCode.Framework.NotEmpty();
        blkCode.Lines
            .NotNull()
            .Assert(x => x.Count > 0, $"{blkCode.Lines} is empty");
    }
}
