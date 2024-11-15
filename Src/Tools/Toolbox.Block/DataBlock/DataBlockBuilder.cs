using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Block;

public class DataBlockBuilder
{
    public string? BlockType { get; set; }
    public string? ClassType { get; set; }
    public string BlockId { get; set; } = Guid.NewGuid().ToString();
    public string? Data { get; set; }
    public string? PrincipleId { get; set; }

    public DataBlockBuilder SetBlockType(string blockType) => this.Action(x => x.BlockType = blockType);
    public DataBlockBuilder SetBlockType<T>() => this.Action(x => x.BlockType = typeof(T).GetTypeName());
    public DataBlockBuilder SetObjectClass(string classType) => this.Action(x => x.ClassType = classType);
    public DataBlockBuilder SetBlockId(string blockId) => this.Action(x => x.BlockId = blockId);
    public DataBlockBuilder SetData(string data) => this.Action(x => Data = data);
    public DataBlockBuilder SetPrincipleId(string principleId) => this.Action(x => PrincipleId = principleId);

    public DataBlockBuilder SetContent<T>(T data) where T : class
    {
        data.NotNull();

        ClassType = data.GetType().GetTypeName();
        Data = data?.ToJson();
        return this;
    }

    public DataBlock Build()
    {
        const string msg = "is required";
        ClassType.NotEmpty(name: msg);

        BlockType ??= ClassType;
        BlockType.Assert(x => IdPatterns.IsName(x), msg);

        BlockId.NotEmpty(name: msg);
        Data.NotEmpty(name: msg);
        PrincipleId.NotNull(name: msg);

        var dataBlock = new DataBlock
        {
            BlockType = BlockType,
            ClassType = ClassType,
            BlockId = BlockId,
            Data = Data,
            PrincipleId = PrincipleId,
        };

        return dataBlock with { Digest = dataBlock.CalculateDigest() };
    }

    public static DataBlock CreateGenesisBlock(string documentId, string principalId, ScopeContext context)
    {
        var marker = new GenesisBlock
        {
            DocumentId = documentId,
            OwnerPrincipalId = principalId,
        };

        GenesisBlockValidator.Validate(marker).ThrowOnError();

        return new DataBlockBuilder()
            .SetBlockType(GenesisBlock.BlockType)
            .SetContent(marker)
            .SetPrincipleId(principalId)
            .Build();
    }

    public static DataBlock CreateAclBlock(IEnumerable<AccessBlock> acls, IEnumerable<RoleAccessBlock> roles, string principalId, ScopeContext context)
    {
        acls.NotNull();
        roles.NotNull();

        var acl = new AclBlock
        {
            AccessRights = acls.ToArray(),
            RoleAccess = roles.ToArray(),
        };

        return CreateAclBlock(acl, principalId, context);
    }

    public static DataBlock CreateAclBlock(AclBlock acl, string principalId, ScopeContext context)
    {
        var o = acl.Validate();
        o.ThrowOnError();

        return new DataBlockBuilder()
            .SetBlockType(AclBlock.BlockType)
            .SetContent(acl)
            .SetPrincipleId(principalId)
            .Build();
    }
}