using Toolbox.Extensions;
using Toolbox.Security;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Block;

public class BlockChainBuilder
{
    public ResourceId? DocumentId { get; set; }
    public string? PrincipleId { get; set; }
    public List<AccessBlock> Access { get; set; } = new List<AccessBlock>();
    public List<RoleAccessBlock> Roles { get; set; } = new List<RoleAccessBlock>();

    public BlockChainBuilder SetDocumentId(ResourceId resourceId) => this.Action(x => DocumentId = resourceId);
    public BlockChainBuilder SetPrincipleId(string principleId) => this.Action(x => PrincipleId = principleId);
    public BlockChainBuilder AddAccess(AclBlock? blockAcl) => this.Action(_ => Access.AddRange(blockAcl?.AccessRights ?? Array.Empty<AccessBlock>()));
    public BlockChainBuilder AddAccess(AccessBlock blockAccess) => this.Action(_ => Access.Add(blockAccess));
    public BlockChainBuilder AddAccess(IEnumerable<AccessBlock> blockAccess) => this.Action(_ => Access.AddRange(blockAccess));
    public BlockChainBuilder AddAccess(RoleAccessBlock blockAccess) => this.Action(_ => Roles.Add(blockAccess));
    public BlockChainBuilder AddAccess(IEnumerable<RoleAccessBlock> blockAccess) => this.Action(_ => Roles.AddRange(blockAccess));

    public async Task<Option<BlockChain>> Build(ISign sign, ScopeContext context)
    {
        DocumentId.NotNull();
        PrincipleId.NotNull();
        Access.NotNull();
        sign.NotNull();

        Option<DataBlock> genesisBlock = await DataBlockBuilder
            .CreateGenesisBlock(DocumentId, PrincipleId, context)
            .Sign(sign, context);

        if (genesisBlock.IsError()) return genesisBlock.ToOptionStatus<BlockChain>();

        var blockChain = new BlockChain();
        blockChain.Add(genesisBlock.Return()).ThrowOnError();

        if (Access.Count > 0)
        {
            Option<DataBlock> acl = await DataBlockBuilder
                .CreateAclBlock(Access, Roles, PrincipleId, context)
                .Sign(sign, context);

            if (acl.IsError()) return acl.ToOptionStatus<BlockChain>();

            blockChain.Add(acl.Return());
        }

        return blockChain;
    }
}

