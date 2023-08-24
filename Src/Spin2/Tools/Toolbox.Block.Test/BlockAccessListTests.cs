using FluentAssertions;
using Toolbox.Extensions;

namespace Toolbox.Block.Test;

public class BlockAccessListTests
{
    [Fact]
    public void EmptyEqual()
    {
        var a1 = new BlockAcl();
        var a2 = new BlockAcl();
        (a1 == a2).Should().BeTrue();
        (a1 != a2).Should().BeFalse();
    }

    [Fact]
    public void EqualSingle()
    {
        var a1 = new BlockAcl
        {
            AccessRights = new BlockAccess { Grant = BlockGrant.Write, Claim = "read.system", BlockType = "blockType", PrincipalId = "user1@domain.com" }.ToEnumerable().ToArray(),
        };

        var a2 = new BlockAcl
        {
            AccessRights = new BlockAccess { Grant = BlockGrant.Write, Claim = "read.system", BlockType = "blockType", PrincipalId = "user1@domain.com" }.ToEnumerable().ToArray(),
        };

        (a1 == a2).Should().BeTrue();
        (a1 != a2).Should().BeFalse();
    }

    [Fact]
    public void NotEqualSingle()
    {
        var a1 = new BlockAcl
        {
            AccessRights = new BlockAccess { Grant = BlockGrant.Read, Claim = "read.system", BlockType = "blockType", PrincipalId = "user1@domain.com" }.ToEnumerable().ToArray(),
        };

        var a2 = new BlockAcl
        {
            AccessRights = new BlockAccess { Grant = BlockGrant.Read, Claim = "read.system", BlockType = "blockType", PrincipalId = "user2@domain.com" }.ToEnumerable().ToArray(),
        };

        (a1 == a2).Should().BeFalse();
        (a1 != a2).Should().BeTrue();
    }

    [Fact]
    public void EqualDouble()
    {
        var a1 = new BlockAcl
        {
            AccessRights = new[]
            {
                new BlockAccess { Grant = BlockGrant.Read, Claim = "read.system", BlockType = "blockType", PrincipalId = "user1@domain.com" },
                new BlockAccess { Grant = BlockGrant.Write, Claim = "write.ledger", BlockType = "blockType", PrincipalId = "user1@domain.com" },
            },
        };

        var a2 = new BlockAcl
        {
            AccessRights = new[]
            {
                new BlockAccess { Grant = BlockGrant.Read, Claim = "read.system", BlockType = "blockType", PrincipalId = "user1@domain.com" },
                new BlockAccess { Grant = BlockGrant.Write, Claim = "write.ledger", BlockType = "blockType", PrincipalId = "user1@domain.com" },
            },
        };

        (a1 == a2).Should().BeTrue();
        (a1 != a2).Should().BeFalse();
    }

    [Fact]
    public void NotEqualDoubleDifferentOrder()
    {
        var a1 = new BlockAcl
        {
            AccessRights = new[]
            {
                new BlockAccess { Claim = "read.system", BlockType = "blockType", PrincipalId = "user1@domain.com" },
                new BlockAccess { Claim = "write.ledger", BlockType = "blockType", PrincipalId = "user1@domain.com" },
            },
        };

        var a2 = new BlockAcl
        {
            AccessRights = new[]
            {
                new BlockAccess { Claim = "write.ledger", BlockType = "blockType", PrincipalId = "user1@domain.com" },
                new BlockAccess { Claim = "read.system", BlockType = "blockType", PrincipalId = "user1@domain.com" },
            },
        };

        (a1 == a2).Should().BeFalse();
        (a1 != a2).Should().BeTrue();
    }

    [Fact]
    public void NotEqualDoubleDifferentQuantity()
    {
        var a1 = new BlockAcl
        {
            AccessRights = new[]
            {
                new BlockAccess { Claim = "read.system", BlockType = "blockType", PrincipalId = "user1@domain.com" },
                new BlockAccess { Claim = "write.ledger", BlockType = "blockType", PrincipalId = "user1@domain.com" },
            },
        };

        var a2 = new BlockAcl
        {
            AccessRights = new[]
            {
                new BlockAccess { Claim = "read.system", BlockType = "blockType", PrincipalId = "user1@domain.com" },
                new BlockAccess { Claim = "write.ledger", BlockType = "blockType", PrincipalId = "user1@domain.com" },
                new BlockAccess { Claim = "write.details", BlockType = "blockType", PrincipalId = "user1@domain.com" },
            },
        };

        (a1 == a2).Should().BeFalse();
        (a1 != a2).Should().BeTrue();
    }
}
