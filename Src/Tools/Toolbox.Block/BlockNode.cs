using System;
using Toolbox.Extensions;
using Toolbox.Security;
using Toolbox.Security.Extensions;
using Toolbox.Tools;

namespace Toolbox.Block;

public record BlockNode
{
    public BlockNode() { }

    public BlockNode(DataBlock blockData)
    {
        blockData.VerifyNotNull(nameof(blockData));

        DataBlock = blockData;
        Digest = BlockNodeExtensions.GetDigest(this);
    }

    public BlockNode(DataBlock blockData, int index, string? previousHash)
    {
        blockData.VerifyNotNull(nameof(blockData));

        DataBlock = blockData;
        Index = index;
        PreviousHash = previousHash;
        Digest = BlockNodeExtensions.GetDigest(this);
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
                $"{blockNode.Index}-{(blockNode.PreviousHash ?? "")}".ToBytes().ToSHA256Hash(),
                blockNode.DataBlock.GetDigest(),
        };

        return hashes.ToMerkleHash();
    }

    public static bool IsValid(this BlockNode blockNode) => blockNode.Digest == blockNode.GetDigest();

    public static void Verify(this BlockNode blockNode)
    {
        blockNode.VerifyNotNull(nameof(blockNode));

        blockNode.DataBlock.VerifyNotNull(nameof(blockNode.DataBlock));
        blockNode.Digest.VerifyNotEmpty(nameof(blockNode.Digest));
    }
}


