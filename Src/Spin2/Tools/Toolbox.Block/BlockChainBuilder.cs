using Toolbox.Extensions;
using Toolbox.Security.Principal;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Block;

public class BlockChainBuilder
{
    public ResourceId? DocumentId { get; set; }
    public string? PrincipleId { get; set; }
    public List<BlockAccess> Access { get; set; } = new List<BlockAccess>();

    public BlockChainBuilder SetDocumentId(ResourceId resourceId) => this.Action(x => DocumentId = resourceId);
    public BlockChainBuilder SetPrincipleId(string principleId) => this.Action(x => PrincipleId = principleId);
    public BlockChainBuilder AddAccess(BlockAcl? blockAcl) => this.Action(_ => Access.AddRange(blockAcl?.AccessRights ?? Array.Empty<BlockAccess>()));
    public BlockChainBuilder AddAccess(BlockAccess blockAccess) => this.Action(_ => Access.Add(blockAccess));
    public BlockChainBuilder AddAccess(IEnumerable<BlockAccess> blockAccess) => this.Action(_ => Access.AddRange(blockAccess));

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
                .CreateAclBlock(Access, PrincipleId, context)
                .Sign(sign, context);

            if (acl.IsError()) return acl.ToOptionStatus<BlockChain>();

            blockChain.Add(acl.Return());
        }

        return blockChain;
    }
}

