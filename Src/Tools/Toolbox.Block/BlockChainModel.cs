using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace Toolbox.Block;

public record BlockChainModel
{
    public IList<BlockNode> Blocks { get; init; } = new List<BlockNode>();
}


public static class BlockChainModelExtensions
{
    public static BlockChainModel Verify(this BlockChainModel blockChainModel)
    {
        blockChainModel.NotNull();
        blockChainModel.Blocks.NotNull();
        blockChainModel.Blocks.ForEach(x => x.Verify());

        return blockChainModel;
    }

    public static IReadOnlyList<T> GetBlockType<T>(this BlockChainModel model, string? blockType = null)
    {
        model.Verify();

        blockType ??= typeof(T).Name;

        return model.Blocks
            .Where(x => x.DataBlock.BlockType == blockType)
            .Select(x => x.DataBlock.ToObject<T>())
            .ToList();
    }
}