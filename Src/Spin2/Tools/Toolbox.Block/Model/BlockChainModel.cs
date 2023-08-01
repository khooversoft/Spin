using Toolbox.Tools;

namespace Toolbox.Block;

public record BlockChainModel
{
    public IList<BlockNode> Blocks { get; init; } = new List<BlockNode>();
}

public static class BlockChainModelExtensions
{
    public static BlockChain ToBlockChain(this BlockChainModel blockChainModel) => new BlockChain(blockChainModel.NotNull().Blocks);
}