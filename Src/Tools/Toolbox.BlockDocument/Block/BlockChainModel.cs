using System.Collections.Generic;
using System.Text;

namespace Toolbox.BlockDocument.Block
{
    public record BlockChainModel
    {
        public BlockChainModel() { }

        public IReadOnlyList<BlockNode> Blocks { get; init; } = new List<BlockNode>();
    }
}
