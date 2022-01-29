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
    public Func<string, Task<string>>? Sign { get; set; }

    public BlockChainBuilder SetSign(Func<string, Task<string>> sign) => this.Action(x => Sign = sign);

    public async Task<BlockChain> Build()
    {
        Sign.VerifyNotNull(nameof(Sign));

        DataBlock genesisBlock = await DataBlockBuilder.CreateGenesisBlock(Sign);

        return new BlockChain()
            .Add(genesisBlock);
    }
}


public static class BlockChainBuilderExtensions
{
    public static BlockChainBuilder SetPrincipleSignature(this BlockChainBuilder subject, IPrincipalSignature principalSignature)
    {
        subject.VerifyNotNull(nameof(subject));
        principalSignature.VerifyNotNull(nameof(principalSignature));

        subject.SetSign(x => Task.FromResult(principalSignature.Sign(x)));

        return subject;
    }
}
