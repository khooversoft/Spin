using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Toolbox.Extensions;
using Toolbox.Security.Principal;
using Toolbox.Types;

namespace Toolbox.Block.Test;

public class BlockChainScalarStreamTests
{
    private const string _owner = "user@domain.com";
    private readonly PrincipalSignature _ownerSignature = new PrincipalSignature(_owner, _owner, "userBusiness@domain.com");
    private readonly PrincipalSignatureCollection _signCollection;
    private readonly ScopeContext _context = new ScopeContext(NullLogger.Instance);

    public BlockChainScalarStreamTests()
    {
        _signCollection = new PrincipalSignatureCollection().Add(_ownerSignature);
    }

    [Fact]
    public async Task GivenSingleDocument_ShouldPass()
    {
        var payloads = new[]
                {
            new Payload { Name = "Name1", Value = 1, Price = 1.5f },
            new Payload { Name = "Name2", Value = 2, Price = 2.5f },
            new Payload { Name = "Name2-offset", Value = 5, Price = 5.5f },
        };

        BlockChain blockChain = await new BlockChainBuilder()
            .SetObjectId("user/tenant/user@domain.com".ToObjectId())
            .SetPrincipleId(_owner)
            .Build(_signCollection, _context)
            .Return();

        Option result = await blockChain.ValidateBlockChain(_signCollection, _context);
        result.StatusCode.IsOk().Should().BeTrue();

        //BlockScalarStream<Payload> stream = blockChain.GetScalarStream<Payload>("ledger");

        //IReadOnlyList<DataBlock> blocks = await payloads
        //    .Select(x => stream.CreateDataBlock(x, _owner).Sign(_signCollection, _context).Return())
        //    .Func(async x => await Task.WhenAll(x));

        //blocks.ForEach(x => stream.Add(x));
        //blockChain.Blocks.Count.Should().Be(4);

        await blockChain.ValidateBlockChain(_signCollection, _context).ThrowOnError();

        //Payload currentPayload = stream.Get().Return();
        //(payloads.Last() == currentPayload).Should().BeTrue();
    }

    private record Payload
    {
        public string? Name { get; set; }
        public int Value { get; set; }
        public float Price { get; set; }
    }
}
