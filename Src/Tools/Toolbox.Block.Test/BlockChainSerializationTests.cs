using Microsoft.Extensions.Logging.Abstractions;
using Toolbox.Extensions;
using Toolbox.Security;
using Toolbox.Tools;
using Toolbox.Tools.Should;
using Toolbox.Types;

namespace Toolbox.Block.Test;

public class BlockChainSerializationTests
{
    private const string _owner = "user@domain.com";
    private readonly PrincipalSignature _ownerSignature = new PrincipalSignature(_owner, _owner, "userBusiness@domain.com");
    private readonly PrincipalSignatureCollection _signCollection;
    private readonly ScopeContext _context = new ScopeContext(NullLogger.Instance);

    public BlockChainSerializationTests()
    {
        _signCollection = new PrincipalSignatureCollection().Add(_ownerSignature);
    }

    [Fact]
    public async Task BlockChainSerializationRoundTrip()
    {
        BlockChain blockChain = await new BlockChainBuilder()
            .SetDocumentId("contrat:domain.com/contract1")
            .SetPrincipleId(_owner)
            .Build(_signCollection, _context)
            .Return();

        Option result = await blockChain.ValidateBlockChain(_signCollection, _context);
        result.IsOk().Should().BeTrue();

        string json = blockChain.ToJson();

        BlockChain? blockChain2 = json.ToObject<BlockChain>();
        blockChain2.NotNull();
        blockChain2!.Count.Should().Be(blockChain.Count);

        Option result2 = await blockChain2!.ValidateBlockChain(_signCollection, _context);
        result2.IsOk().Should().BeTrue();
    }
}
