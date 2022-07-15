using System;
using System.Collections.Generic;
using System.Linq;
using Toolbox.Extensions;
using Toolbox.Security;
using Toolbox.Tools;

namespace Toolbox.Block
{
    public static class Extensions
    {
        public static BlockChain Add<T>(this BlockChain blockChain, T value, string principleId)
        {
            blockChain.NotNull();
            value.NotNull();
            principleId.NotEmpty();

            blockChain += new DataBlockBuilder()
                .SetTimeStamp(DateTime.UtcNow)
                .SetBlockType(value.GetType().Name)
                .SetBlockId(blockChain.Blocks.Count.ToString())
                .SetData(value.ToJson())
                .SetPrincipleId(principleId)
                .Build();

            return blockChain;
        }

        public static IReadOnlyList<DataBlock> FindBlockType(this BlockChain blockChain, string blockType)
        {
            blockChain.NotNull();

            return blockChain
                .Blocks.Where(x => x.DataBlock.BlockType == blockType)
                .Select(x => x.DataBlock)
                .ToList();
        }

        public static IReadOnlyList<T> FindBlockType<T>(this BlockChain blockChain)
        {
            blockChain.NotNull();

            return blockChain
                .Blocks.Where(x => x.DataBlock.BlockType == typeof(T).Name)
                .Select(x => x.DataBlock.ToObject<T>())
                .ToList();
        }

        public static BlockChain ToBlockChain(this BlockChainModel blockChainModel)
        {
            blockChainModel.NotNull();

            return new BlockChain(blockChainModel.Blocks);
        }

        public static BlockChainModel ToBlockChainModel(this BlockChain blockChain)
        {
            blockChain.NotNull();

            return new BlockChainModel { Blocks = blockChain.Blocks.ToList() };
        }

        public static MerkleTree ToMerkleTree(this BlockChain blockChain)
        {
            return new MerkleTree()
                .Append(blockChain.Blocks.Select(x => x.Digest).ToArray());
        }
    }
}