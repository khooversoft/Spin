using Toolbox.Block.Container;

namespace Toolbox.Block.Serialization;

public record BlockChainModel
{
    public IList<BlockNode> Blocks { get; init; } = new List<BlockNode>();
}
