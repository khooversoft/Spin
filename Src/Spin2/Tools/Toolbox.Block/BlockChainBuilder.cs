using Toolbox.Extensions;
using Toolbox.Security.Principal;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Block;

public class BlockChainBuilder
{
    public ObjectId? ObjectId { get; set; }
    public string? PrincipleId { get; set; }
    public IList<BlockAccess> Access { get; set; } = new List<BlockAccess>();

    public BlockChainBuilder SetObjectId(ObjectId objectId) => this.Action(x => ObjectId = objectId);
    public BlockChainBuilder SetPrincipleId(string principleId) => this.Action(x => PrincipleId = principleId);
    public BlockChainBuilder AddAccess(BlockAccess blockAccess) => this.Action(_ => Access.Add(blockAccess));
    public BlockChainBuilder AddAccess(bool writeGrant, string? claim, PrincipalId principalId)
    {
        Access.Add(new BlockAccess { WriteGrant = true, Claim = claim, PrincipalId = principalId.ToString() });
        return this;
    }

    public async Task<Option<BlockChain>> Build(ISign sign, ScopeContext context)
    {
        ObjectId.NotNull();
        PrincipleId.NotEmpty();
        Access.NotNull();
        sign.NotNull();

        Option<DataBlock> genesisBlock = await DataBlockBuilder
            .CreateGenesisBlock(ObjectId.ToString(), PrincipleId, context)
            .Sign(sign, context);

        if (genesisBlock.IsError()) return genesisBlock.ToOption<BlockChain>();

        var blockChain = new BlockChain();
        blockChain.Add(genesisBlock.Return()).ThrowOnError();

        if (Access.Count > 0)
        {
            Option<DataBlock> acl = await DataBlockBuilder
                .CreateAclBlock(Access, PrincipleId, context)
                .Sign(sign, context);

            blockChain.Add(acl.Return());
        }

        return blockChain;
    }
}

