using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Extensions;
using Toolbox.Security.Sign;
using Toolbox.Tools;

namespace Toolbox.Block
{
    public class BlockChainBuilder
    {
        public IPrincipleSignature? PrincipleSignature { get; set; }

        public BlockChainBuilder SetPrincipleSignature(IPrincipleSignature principleSignatures) => this.Action(x => x.PrincipleSignature = principleSignatures);

        public BlockChain Build()
        {
            PrincipleSignature.VerifyNotNull(nameof(PrincipleSignature));

            DataBlock genesisBlock = DataBlockBuilder.CreateGenesisBlock(PrincipleSignature);

            return new BlockChain()
                .Add(genesisBlock);
        }
    }
}
