using Toolbox.Extensions;
using Toolbox.Tools;

namespace Toolbox.Block.Test;

public class BlockAccessListTests
{
    [Fact]
    public void EmptyEqual()
    {
        var a1 = new AclBlock();
        var a2 = new AclBlock();
        (a1 == a2).BeTrue();
        (a1 != a2).BeFalse();
    }

    [Fact]
    public void EqualSingle()
    {
        var a1 = new AclBlock
        {
            AccessRights = new AccessBlock { Grant = BlockGrant.Write, Claim = "read.system", BlockType = "blockType", PrincipalId = "user1@domain.com" }.ToEnumerable().ToArray(),
        };

        var a2 = new AclBlock
        {
            AccessRights = new AccessBlock { Grant = BlockGrant.Write, Claim = "read.system", BlockType = "blockType", PrincipalId = "user1@domain.com" }.ToEnumerable().ToArray(),
        };

        (a1 == a2).BeTrue();
        (a1 != a2).BeFalse();
    }

    [Fact]
    public void NotEqualSingle()
    {
        var a1 = new AclBlock
        {
            AccessRights = new AccessBlock { Grant = BlockGrant.Read, Claim = "read.system", BlockType = "blockType", PrincipalId = "user1@domain.com" }.ToEnumerable().ToArray(),
        };

        var a2 = new AclBlock
        {
            AccessRights = new AccessBlock { Grant = BlockGrant.Read, Claim = "read.system", BlockType = "blockType", PrincipalId = "user2@domain.com" }.ToEnumerable().ToArray(),
        };

        (a1 == a2).BeFalse();
        (a1 != a2).BeTrue();
    }

    [Fact]
    public void EqualDouble()
    {
        var a1 = new AclBlock
        {
            AccessRights = new[]
            {
                new AccessBlock { Grant = BlockGrant.Read, Claim = "read.system", BlockType = "blockType", PrincipalId = "user1@domain.com" },
                new AccessBlock { Grant = BlockGrant.Write, Claim = "write.ledger", BlockType = "blockType", PrincipalId = "user1@domain.com" },
            },
        };

        var a2 = new AclBlock
        {
            AccessRights = new[]
            {
                new AccessBlock { Grant = BlockGrant.Read, Claim = "read.system", BlockType = "blockType", PrincipalId = "user1@domain.com" },
                new AccessBlock { Grant = BlockGrant.Write, Claim = "write.ledger", BlockType = "blockType", PrincipalId = "user1@domain.com" },
            },
        };

        (a1 == a2).BeTrue();
        (a1 != a2).BeFalse();
    }

    [Fact]
    public void NotEqualDoubleDifferentOrder()
    {
        var a1 = new AclBlock
        {
            AccessRights = new[]
            {
                new AccessBlock { Claim = "read.system", BlockType = "blockType", PrincipalId = "user1@domain.com" },
                new AccessBlock { Claim = "write.ledger", BlockType = "blockType", PrincipalId = "user1@domain.com" },
            },
        };

        var a2 = new AclBlock
        {
            AccessRights = new[]
            {
                new AccessBlock { Claim = "write.ledger", BlockType = "blockType", PrincipalId = "user1@domain.com" },
                new AccessBlock { Claim = "read.system", BlockType = "blockType", PrincipalId = "user1@domain.com" },
            },
        };

        (a1 == a2).BeFalse();
        (a1 != a2).BeTrue();
    }

    [Fact]
    public void NotEqualDoubleDifferentQuantity()
    {
        var a1 = new AclBlock
        {
            AccessRights = new[]
            {
                new AccessBlock { Claim = "read.system", BlockType = "blockType", PrincipalId = "user1@domain.com" },
                new AccessBlock { Claim = "write.ledger", BlockType = "blockType", PrincipalId = "user1@domain.com" },
            },
        };

        var a2 = new AclBlock
        {
            AccessRights = new[]
            {
                new AccessBlock { Claim = "read.system", BlockType = "blockType", PrincipalId = "user1@domain.com" },
                new AccessBlock { Claim = "write.ledger", BlockType = "blockType", PrincipalId = "user1@domain.com" },
                new AccessBlock { Claim = "write.details", BlockType = "blockType", PrincipalId = "user1@domain.com" },
            },
        };

        (a1 == a2).BeFalse();
        (a1 != a2).BeTrue();
    }
}
