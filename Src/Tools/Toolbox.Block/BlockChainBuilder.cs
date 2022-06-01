using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Extensions;
using Toolbox.Security.Sign;
using Toolbox.Tools;

namespace Toolbox.Block;

public class BlockChainBuilder
{
    public string? PrincipleId { get; set; }

    public BlockChainBuilder SetPrincipleId(string principleId) => this.Action(x => PrincipleId = principleId);

    public BlockChain Build()
    {
        PrincipleId.NotEmpty();

        DataBlock genesisBlock = DataBlockBuilder.CreateGenesisBlock(PrincipleId);

        return new BlockChain()
            .Add(genesisBlock);
    }
}

