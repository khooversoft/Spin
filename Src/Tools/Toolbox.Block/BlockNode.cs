using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Block;

public record BlockNode
{
    public BlockNode() { }

    public BlockNode(DataBlock blockData)
    {
        blockData.NotNull();

        DataBlock = blockData;
        Digest = this.GetDigest();
    }

    public BlockNode(DataBlock blockData, int index, string? previousHash)
    {
        blockData.NotNull();

        DataBlock = blockData;
        Index = index;
        PreviousHash = previousHash;
        Digest = this.GetDigest();
    }

    public DataBlock DataBlock { get; init; } = null!;
    public int Index { get; init; }
    public string? PreviousHash { get; init; }
    public string Digest { get; init; } = null!;
}


public static class BlockNodeExtensions
{
    public static string GetDigest(this BlockNode blockNode)
    {
        var hashes = new string[]
        {
            $"{blockNode.Index}-{blockNode.PreviousHash ?? ""}".ToBytes().ToHexHash(),
            blockNode.DataBlock.CalculateDigest(),
        };

        return hashes.ToMerkleHash();
    }

    public static bool IsValid(this BlockNode blockNode) => blockNode.Digest == blockNode.GetDigest();

    public static void Verify(this BlockNode blockNode)
    {
        blockNode.NotNull();

        blockNode.DataBlock.NotNull();
        blockNode.Digest.NotEmpty();
    }
}
