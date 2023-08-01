using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Toolbox.Extensions;
using Toolbox.Security.Principal;
using Toolbox.Types;

namespace Toolbox.Block.Test;

public class BlockChainSignAccessTests
{
    private readonly ScopeContext _context = new ScopeContext(NullLogger.Instance);

    [Fact]
    public async Task SingleNodeWithAccessWrites()
    {
        const string issuer = "user@domain.com";
        const string objectId = $"user/tenant/{issuer}";
        var now = DateTime.UtcNow;

        IPrincipalSignature principleSignature = new PrincipalSignature(issuer, issuer, "userBusiness@domain.com");

        BlockChain blockChain = await new BlockChainBuilder()
            .SetObjectId(objectId.ToObjectId())
            .SetPrincipleId(issuer)
            .AddAccess(new BlockAccess { Grant = BlockGrant.Write, BlockType = typeof(Payload1).GetTypeName(), PrincipalId = issuer })
            .Build(principleSignature, _context)
            .Return();

        var p1 = new Payload1
        {
            Name = "name1",
            Last = "last1",
        };

        var data = await new DataBlockBuilder()
            .SetTimeStamp(now)
            .SetData(p1)
            .SetBlockType<Payload1>()
            .SetPrincipleId(issuer)
            .Build()
            .Sign(principleSignature, _context)
            .Return();

        Option status = blockChain.Add(data);
        status.StatusCode.IsOk().Should().BeTrue();
        blockChain.Count.Should().Be(3);
    }
    
    [Fact]
    public async Task MultipleNodeWithAccessWrites()
    {
        const string issuer = "user@domain.com";
        const string objectId = $"user/tenant/{issuer}";
        const string issuer2 = "user2@domain.com";
        var now = DateTime.UtcNow;

        IPrincipalSignature principleSignature = new PrincipalSignature(issuer, issuer, "userBusiness@domain.com");
        IPrincipalSignature principleSignature2 = new PrincipalSignature(issuer2, issuer2, "userBusiness@domain.com");

        BlockChain blockChain = await new BlockChainBuilder()
            .SetObjectId(objectId.ToObjectId())
            .SetPrincipleId(issuer)
            .AddAccess(new BlockAccess { Grant = BlockGrant.Write, BlockType = typeof(Payload1).GetTypeName(), PrincipalId = issuer })
            .AddAccess(new BlockAccess { Grant = BlockGrant.Write, BlockType = typeof(Payload2).GetTypeName(), PrincipalId = issuer2 })
            .Build(principleSignature, _context)
            .Return();

        var p1 = new Payload1
        {
            Name = "name1",
            Last = "last1",
        };

        var data = await new DataBlockBuilder()
            .SetTimeStamp(now)
            .SetData(p1)
            .SetPrincipleId(issuer)
            .Build()
            .Sign(principleSignature, _context)
            .Return();

        var p2 = new Payload2
        {
            Description = "description1",
        };

        var data2 = await new DataBlockBuilder()
            .SetTimeStamp(now)
            .SetData(p2)
            .SetPrincipleId(issuer2)
            .Build()
            .Sign(principleSignature2, _context)
            .Return();

        Option status = blockChain.Add(data2);
        status.StatusCode.IsOk().Should().BeTrue();
        blockChain.Count.Should().Be(3);
    }

    [Fact]
    public async Task SingleNodeWithAccessWritesFails()
    {
        const string issuer = "user@domain.com";
        const string issuer2 = "user2@domain.com";
        const string issuer3 = "user3@domain.com";
        const string objectId = $"user/tenant/{issuer}";
        var now = DateTime.UtcNow;

        IPrincipalSignature principleSignature = new PrincipalSignature(issuer, issuer, "userBusiness@domain.com");
        IPrincipalSignature principleSignature3 = new PrincipalSignature(issuer3, issuer3, "userBusiness@domain.com");

        BlockChain blockChain = await new BlockChainBuilder()
            .SetObjectId(objectId.ToObjectId())
            .SetPrincipleId(issuer)
            .AddAccess(new BlockAccess { Grant = BlockGrant.Write, BlockType = typeof(Payload1).GetTypeName(), PrincipalId = issuer2 })
            .Build(principleSignature, _context)
            .Return();

        var p1 = new Payload1
        {
            Name = "name1",
            Last = "last1",
        };

        var data = await new DataBlockBuilder()
            .SetTimeStamp(now)
            .SetData(p1)
            .SetBlockType<Payload1>()
            .SetPrincipleId(issuer3)
            .Build()
            .Sign(principleSignature3, _context)
            .Return();

        Option status = blockChain.Add(data);
        status.StatusCode.IsOk().Should().BeFalse();
    }

    [Fact]
    public async Task MultipleNodeWithAccessWritesFailed()
    {
        const string issuer = "user@domain.com";
        const string objectId = $"user/tenant/{issuer}";
        const string issuer2 = "user2@domain.com";
        var now = DateTime.UtcNow;

        IPrincipalSignature principleSignature = new PrincipalSignature(issuer, issuer, "userBusiness@domain.com");
        IPrincipalSignature principleSignature2 = new PrincipalSignature(issuer2, issuer2, "userBusiness@domain.com");

        BlockChain blockChain = await new BlockChainBuilder()
            .SetObjectId(objectId.ToObjectId())
            .SetPrincipleId(issuer)
            .Build(principleSignature, _context)
            .Return();

        var p1 = new Payload1
        {
            Name = "name1",
            Last = "last1",
        };

        var data = await new DataBlockBuilder()
            .SetTimeStamp(now)
            .SetData(p1)
            .SetPrincipleId(issuer)
            .Build()
            .Sign(principleSignature, _context)
            .Return();

        Option status = blockChain.Add(data);
        status.StatusCode.IsOk().Should().BeTrue();

        var p2 = new Payload2
        {
            Description = "description1",
        };

        var data2 = await new DataBlockBuilder()
            .SetTimeStamp(now)
            .SetData(p2)
            .SetPrincipleId(issuer2)
            .Build()
            .Sign(principleSignature2, _context)
            .Return();

        Option status2 = blockChain.Add(data2);
        status2.StatusCode.IsOk().Should().BeFalse();
        blockChain.Count.Should().Be(2);
    }

    private record Payload1
    {
        public string Name { get; init; } = null!;
        public string Last { get; init; } = null!;
    }

    private record Payload2
    {
        public string Description { get; init; } = null!;
    }
}
