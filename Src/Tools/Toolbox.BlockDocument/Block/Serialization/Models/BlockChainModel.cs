using System.Collections.Generic;
using System.Text;

namespace Toolbox.BlockDocument
{
    public record BlockChainModel
    {
      public IReadOnlyList<BlockChainNodeModel>? Blocks { get; init; }
    }
}
