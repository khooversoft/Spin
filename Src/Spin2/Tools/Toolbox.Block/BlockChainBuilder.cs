using Toolbox.Extensions;
using Toolbox.Security.Principal;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Block;

public class BlockChainBuilder
{
    public ObjectId? ObjectId { get; set; }
    public string? PrincipleId { get; set; }

    public BlockChainBuilder SetObjectId(ObjectId objectId) => this.Action(x => ObjectId = objectId);
    public BlockChainBuilder SetPrincipleId(string principleId) => this.Action(x => PrincipleId = principleId);

    public async Task<Option<BlockChain>> Build(ISign sign, ScopeContext context)
    {
        ObjectId.NotNull();
        PrincipleId.NotEmpty();
        sign.NotNull();

        Option<DataBlock> genesisBlock = await DataBlockBuilder
            .CreateGenesisBlock(ObjectId.ToString(), PrincipleId)
            .Sign(sign, context);

        if (genesisBlock.IsError()) return genesisBlock.ToOption<BlockChain>();

        var blockChain = new BlockChain().Add(genesisBlock.Return());
        return blockChain;
    }
}

