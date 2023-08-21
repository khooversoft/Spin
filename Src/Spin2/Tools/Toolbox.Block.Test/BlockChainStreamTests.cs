using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Toolbox.Extensions;
using Toolbox.Security.Principal;
using Toolbox.Types;

namespace Toolbox.Block.Test;

public class BlockChainStreamTests
{
    private const string _owner = "user@domain.com";
    private readonly PrincipalSignature _ownerSignature = new PrincipalSignature(_owner, _owner, "userBusiness@domain.com");
    private readonly PrincipalSignatureCollection _signCollection;
    private readonly ScopeContext _context = new ScopeContext(NullLogger.Instance);

    public BlockChainStreamTests()
    {
        _signCollection = new PrincipalSignatureCollection().Add(_ownerSignature);
    }

    [Fact]
    public async Task GivenSingleDocumentWithStream()
    {
        var payloads = new[]
        {
            new Payload { Name = "Name1", Value = 1, Price = 1.5f },
            new Payload { Name = "Name2", Value = 2, Price = 2.5f },
            new Payload { Name = "Name2-offset", Value = 5, Price = 5.5f },
        };

        BlockChain blockChain = await new BlockChainBuilder()
            .SetDocumentId("user/tenant/user@domain.com")
            .SetPrincipleId(_owner)
            .Build(_signCollection, _context)
            .Return();

        Option result = await blockChain.ValidateBlockChain(_signCollection, _context);
        result.StatusCode.IsOk().Should().BeTrue();

        BlockWriter<Payload> writer = blockChain.GetWriter<Payload>("ledger", _owner).Return();

        IReadOnlyList<DataBlock> blocks = await payloads
            .Select(x => writer.CreateDataBlock(x, _owner).Sign(_signCollection, _context).Return())
            .Func(async x => await Task.WhenAll(x));

        blocks.ForEach(x => writer.Add(x));
        blockChain.Count.Should().Be(4);

        await blockChain.ValidateBlockChain(_signCollection, _context).ThrowOnError();

        BlockReader<Payload> reader = blockChain.GetReader<Payload>("ledger", _owner).Return();
        IReadOnlyList<Payload> blockPayloads = reader.List();
        blockPayloads.Count.Should().Be(3);

        payloads.SequenceEqual(blockPayloads).Should().BeTrue();
    }

    private record Payload
    {
        public string? Name { get; set; }
        public int Value { get; set; }
        public float Price { get; set; }
    }
}
