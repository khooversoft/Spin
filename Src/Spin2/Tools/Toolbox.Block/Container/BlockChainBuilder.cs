using Toolbox.Extensions;
using Toolbox.Tools;

namespace Toolbox.Block.Container;

public class BlockChainBuilder
{
    public string? PrincipleId { get; set; }

    public BlockChainBuilder SetPrincipleId(string principleId) => this.Action(x => PrincipleId = principleId);

    public BlockChain Build()
    {
        PrincipleId.NotEmpty();

        DataBlock genesisBlock = DataBlockBuilder.CreateGenesisBlock(PrincipleId);

        return new BlockChain().Add(genesisBlock);
    }
}

