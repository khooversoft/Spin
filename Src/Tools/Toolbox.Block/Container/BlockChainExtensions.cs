using System;
using System.Collections.Generic;
using System.Linq;
using Toolbox.Block.Application;
using Toolbox.Block.Serialization;
using Toolbox.Extensions;
using Toolbox.Security;
using Toolbox.Tools;

namespace Toolbox.Block.Container;

public static class BlockChainExtensions
{
    public static BlockChain Add<T>(this BlockChain blockChain, T value, string principleId, string? blockType = null) where T : class
    {
        blockChain.NotNull();
        value.NotNull();
        principleId.NotEmpty();
        blockType.Assert(x => x == null | !x.IsEmpty(), $"{nameof(blockType)} cannot be empty, null is valid");

        blockChain += new DataBlockBuilder()
            .SetTimeStamp(DateTime.UtcNow)
            .SetBlockType(blockType ?? value.GetType().GetTypeName())
            .SetData(value)
            .SetPrincipleId(principleId)
            .Build();

        return blockChain;
    }

    public static IReadOnlyList<T> GetTypedBlocks<T>(this BlockChain blockChain) => blockChain.NotNull()
        .GetTypedBlocks(typeof(T).GetTypeName())
        .Select(x => x.ToObject<T>())
        .ToArray();

    public static IReadOnlyList<DataBlock> GetTypedBlocks(this BlockChain blockChain, string blockType) => blockChain.NotNull()
        .Blocks
        .Where(x => x.DataBlock.BlockType == blockType)
        .Select(x => x.DataBlock)
        .ToList();

    public static BlockChain ToBlockChain(this BlockChainModel blockChainModel)
    {
        blockChainModel.NotNull();

        return new BlockChain(blockChainModel.Blocks);
    }

    public static BlockChainModel ToBlockChainModel(this BlockChain blockChain) => new BlockChainModel
    {
        Blocks = blockChain.NotNull().Blocks.ToList()
    };

    public static MerkleTree ToMerkleTree(this BlockChain blockChain)
    {
        return new MerkleTree()
            .Append(blockChain.Blocks.Select(x => x.Digest).ToArray());
    }
}