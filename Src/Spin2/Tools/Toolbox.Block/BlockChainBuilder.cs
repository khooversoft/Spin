using Toolbox.Extensions;
using Toolbox.Security.Principal;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Block;

public class BlockChainBuilder
{
    public ObjectId? ObjectId { get; set; }
    public PrincipalId? PrincipleId { get; set; }
    public List<BlockAccess> Access { get; set; } = new List<BlockAccess>();

    public BlockChainBuilder SetObjectId(ObjectId objectId) => this.Action(x => ObjectId = objectId);
    public BlockChainBuilder SetPrincipleId(PrincipalId principleId) => this.Action(x => PrincipleId = principleId);
    public BlockChainBuilder AddAccess(BlockAcl? blockAcl) => this.Action(_ => Access.AddRange(blockAcl?.Items ?? Array.Empty<BlockAccess>()));
    public BlockChainBuilder AddAccess(BlockAccess blockAccess) => this.Action(_ => Access.Add(blockAccess));

    public async Task<Option<BlockChain>> Build(ISign sign, ScopeContext context)
    {
        ObjectId.NotNull();
        PrincipleId.NotNull();
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

            if (acl.IsError()) return acl.ToOption<BlockChain>();

            blockChain.Add(acl.Return());
        }

        return blockChain;
    }
}

