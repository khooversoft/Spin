using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            Items = new BlockAccess { Claim = "read.system", PrincipalId = "user1@domain.com" }.ToEnumerable().ToArray(),
        };

        var a2 = new BlockAcl
        {
            Items = new BlockAccess { Claim = "read.system", PrincipalId = "user1@domain.com" }.ToEnumerable().ToArray(),
        };

        (a1 == a2).Should().BeTrue();
        (a1 != a2).Should().BeFalse();
    }

    [Fact]
    public void NotEqualSingle()
    {
        var a1 = new BlockAcl
        {
            Items = new BlockAccess { Claim = "read.system", PrincipalId = "user1@domain.com" }.ToEnumerable().ToArray(),
        };

        var a2 = new BlockAcl
        {
            Items = new BlockAccess { Claim = "read.system", PrincipalId = "user2@domain.com" }.ToEnumerable().ToArray(),
        };

        (a1 == a2).Should().BeFalse();
        (a1 != a2).Should().BeTrue();
    }

    [Fact]
    public void EqualDouble()
    {
        var a1 = new BlockAcl
        {
            Items = new[]
            {
                new BlockAccess { Claim = "read.system", PrincipalId = "user1@domain.com" },
                new BlockAccess { Claim = "write.ledger", PrincipalId = "user1@domain.com" },
            },
        };

        var a2 = new BlockAcl
        {
            Items = new[]
            {
                new BlockAccess { Claim = "read.system", PrincipalId = "user1@domain.com" },
                new BlockAccess { Claim = "write.ledger", PrincipalId = "user1@domain.com" },
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
            Items = new[]
            {
                new BlockAccess { Claim = "read.system", PrincipalId = "user1@domain.com" },
                new BlockAccess { Claim = "write.ledger", PrincipalId = "user1@domain.com" },
            },
        };

        var a2 = new BlockAcl
        {
            Items = new[]
            {
                new BlockAccess { Claim = "write.ledger", PrincipalId = "user1@domain.com" },
                new BlockAccess { Claim = "read.system", PrincipalId = "user1@domain.com" },
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
            Items = new[]
            {
                new BlockAccess { Claim = "read.system", PrincipalId = "user1@domain.com" },
                new BlockAccess { Claim = "write.ledger", PrincipalId = "user1@domain.com" },
            },
        };

        var a2 = new BlockAcl
        {
            Items = new[]
            {
                new BlockAccess { Claim = "read.system", PrincipalId = "user1@domain.com" },
                new BlockAccess { Claim = "write.ledger", PrincipalId = "user1@domain.com" },
                new BlockAccess { Claim = "write.details", PrincipalId = "user1@domain.com" },
            },
        };

        (a1 == a2).Should().BeFalse();
        (a1 != a2).Should().BeTrue();
    }
}
