using Toolbox.Block;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace Contract.sdk.Models;

public record BlkCollection
{
    public IReadOnlyList<DataBlock> Blocks { get; init; } = Array.Empty<DataBlock>();
}


public static class BlkCollectionExtensions
{
    public static void Verify(this BlkCollection subject)
    {
        subject.NotNull();
        subject.Blocks.NotNull();

        subject.Blocks.ForEach(x => x.Verify());
    }
}