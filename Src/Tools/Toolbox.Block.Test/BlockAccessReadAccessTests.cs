using Microsoft.Extensions.Logging.Abstractions;
using Toolbox.Extensions;
using Toolbox.Security;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Block.Test;

public class BlockAccessReadAccessTests
{
    private readonly ScopeContext _context = new ScopeContext(NullLogger.Instance);

    [Fact]
    public async Task MultipleNodeWithAccessWrites()
    {
        const string issuer = "user@domain.com";
        const string documentId = "contract:domain.com/contract1";
        const string issuer2 = "user2@domain.com";

        IPrincipalSignature principleSignature = new PrincipalSignature(issuer, issuer, "userBusiness@domain.com");
        IPrincipalSignature principleSignature2 = new PrincipalSignature(issuer2, issuer2, "userBusiness@domain.com");

        BlockChain blockChain = await new BlockChainBuilder()
            .SetDocumentId(documentId)
            .SetPrincipleId(issuer)
            .AddAccess(new AccessBlock { Grant = BlockGrant.Write, BlockType = typeof(Payload2).GetTypeName(), PrincipalId = issuer2 })
            .Build(principleSignature, _context)
            .Return();

        var p1 = new Payload1 { Name = "name1", Last = "last1" };

        var data = await new DataBlockBuilder()
            .SetContent(p1)
            .SetPrincipleId(issuer)
            .Build()
            .Sign(principleSignature, _context)
            .Return();

        blockChain.Add(data).StatusCode.IsOk().BeTrue();

        var p2 = new Payload2 { Description = "description1" };

        var data2 = await new DataBlockBuilder()
            .SetContent(p2)
            .SetPrincipleId(issuer2)
            .Build()
            .Sign(principleSignature2, _context)
            .Return();

        blockChain.Add(data2).StatusCode.IsOk().BeTrue();
        blockChain.Count.Be(4);
    }

    [Fact]
    public async Task MultipleNodeWithAccessWriteButNotRead()
    {
        const string issuer = "user@domain.com";
        const string documentId = "contract:domain.com/contract1";
        const string issuer2 = "user2@domain.com";

        IPrincipalSignature principleSignature = new PrincipalSignature(issuer, issuer, "userBusiness@domain.com");
        IPrincipalSignature principleSignature2 = new PrincipalSignature(issuer2, issuer2, "userBusiness@domain.com");

        BlockChain blockChain = await new BlockChainBuilder()
            .SetDocumentId(documentId)
            .SetPrincipleId(issuer)
            .Build(principleSignature, _context)
            .Return();

        var acl = new AclBlock
        {
            AccessRights = new AccessBlock { Grant = BlockGrant.Write, BlockType = typeof(Payload2).GetTypeName(), PrincipalId = issuer2 }
                .ToEnumerable()
                .ToArray(),
        };

        Option<DataBlock> aclBlock = await DataBlockBuilder
            .CreateAclBlock(acl, issuer, _context)
            .Sign(principleSignature, _context);

        aclBlock.StatusCode.IsOk().BeTrue();

        blockChain.Add(aclBlock.Return()).StatusCode.IsOk().BeTrue();

        // Check write access
        var p1 = new Payload1 { Name = "name1", Last = "last1" };

        var data = await new DataBlockBuilder()
            .SetContent(p1)
            .SetPrincipleId(issuer)
            .Build()
            .Sign(principleSignature, _context)
            .Return();

        blockChain.Add(data).StatusCode.IsOk().BeTrue();

        var p2 = new Payload2 { Description = "description1" };

        var data2 = await new DataBlockBuilder()
            .SetContent(p2)
            .SetPrincipleId(issuer2)
            .Build()
            .Sign(principleSignature2, _context)
            .Return();

        blockChain.Add(data2).StatusCode.IsOk().BeTrue();
        blockChain.Count.Be(4);

        // Check read access
        var stream1 = blockChain.HasAccess(issuer2, BlockGrant.Read, nameof(Payload1));
        stream1.IsError().BeTrue();
        var stream2 = blockChain.HasAccess(issuer2, BlockGrant.Read, nameof(Payload2));
        stream2.IsError().BeTrue();

        var streamOwner1 = blockChain.Filter<Payload1>(issuer);
        streamOwner1.IsOk().BeTrue();
        streamOwner1.Return().ToList().Count.Be(1);

        var streamOwner2 = blockChain.Filter<Payload2>(issuer);
        streamOwner2.IsOk().BeTrue();
        streamOwner2.Return().ToList().Count.Be(1);
    }

    [Fact]
    public async Task MultipleNodeWithAccessWriteReadWrite()
    {
        const string issuer = "user@domain.com";
        const string documentId = "contract:domain.com/contract1";
        const string issuer2 = "user2@domain.com";

        IPrincipalSignature principleSignature = new PrincipalSignature(issuer, issuer, "userBusiness@domain.com");
        IPrincipalSignature principleSignature2 = new PrincipalSignature(issuer2, issuer2, "userBusiness@domain.com");

        BlockChain blockChain = await new BlockChainBuilder()
            .SetDocumentId(documentId)
            .SetPrincipleId(issuer)
            .AddAccess(new AccessBlock { Grant = BlockGrant.ReadWrite, BlockType = typeof(Payload2).GetTypeName(), PrincipalId = issuer2 })
            .Build(principleSignature, _context)
            .Return();

        // Check write access
        var p1 = new Payload1 { Name = "name1", Last = "last1" };

        var data = await new DataBlockBuilder()
            .SetContent(p1)
            .SetPrincipleId(issuer)
            .Build()
            .Sign(principleSignature, _context)
            .Return();

        blockChain.Add(data).StatusCode.IsOk().BeTrue();

        var p2 = new Payload2 { Description = "description1" };

        var data2 = await new DataBlockBuilder()
            .SetContent(p2)
            .SetPrincipleId(issuer2)
            .Build()
            .Sign(principleSignature2, _context)
            .Return();

        blockChain.Add(data2).StatusCode.IsOk().BeTrue();
        blockChain.Count.Be(4);

        // Check read access
        var stream1 = blockChain.Filter<Payload1>(issuer2);
        stream1.IsError().BeTrue();

        var stream3 = blockChain.Filter<Payload2>(issuer2);
        stream3.IsOk().BeTrue();
        stream3.Return().ToList().Count.Be(1);

        var streamOwner1 = blockChain.Filter<Payload1>(issuer);
        streamOwner1.IsOk().BeTrue();
        streamOwner1.Return().ToList().Count.Be(1);

        var streamOwner2 = blockChain.Filter<Payload2>(issuer);
        streamOwner2.IsOk().BeTrue();
        streamOwner2.Return().ToList().Count.Be(1);
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
