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
    public static void Verify(this BlockChainModel blockChainModel)
    {
        blockChainModel.VerifyNotNull(nameof(blockChainModel));
        blockChainModel.Blocks.VerifyNotNull(nameof(blockChainModel.Blocks));
        blockChainModel.Blocks.ForEach(x => x.Verify());
    }
}