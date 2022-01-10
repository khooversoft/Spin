using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Toolbox.Block;

public record BlockChainModel
{
    public BlockChainModel() { }

    public IReadOnlyList<BlockNode> Blocks { get; init; } = new List<BlockNode>();
}
