using Microsoft.Extensions.Logging.Abstractions;
using Toolbox.Block;
using Toolbox.Data;
using Toolbox.Extensions;
using Toolbox.Security;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Block.Test
{
    public class BlockChainSignTests
    {
        private readonly ScopeContext _context = new ScopeContext(NullLogger.Instance);

        [Fact]
        public async Task EmptyBlockChain()
        {
            const string issuer = "user@domain.com";

            IPrincipalSignature principleSignature = new PrincipalSignature(issuer, issuer, "userBusiness@domain.com");

            BlockChain blockChain = await new BlockChainBuilder()
                .SetDocumentId("contract:domain.com/contract1")
                .SetPrincipleId(issuer)
                .Build(principleSignature, _context)
                .Return();

            Option result = await blockChain.ValidateBlockChain(principleSignature, _context);
            result.StatusCode.IsOk().BeTrue();

            blockChain.GetDigest().NotEmpty();
        }

        [Fact]
        public async Task AppendSingleNode()
        {
            const string issuer = "user@domain.com";
            const string documentId = "contract:domain.com/contract1";

            IPrincipalSignature principleSignature = new PrincipalSignature(issuer, issuer, "userBusiness@domain.com");

            var dataPayload = new
            {
                Name = "Name",
                Type = "Type",
                Author = "Author",
                Data = "Data"
            };

            string payloadJson = dataPayload.ToJson();

            DataBlock data = await new DataBlockBuilder()
                .SetBlockId("blockId")
                .SetBlockType("blockType")
                .SetObjectClass("blockClass")
                .SetData(payloadJson)
                .SetPrincipleId(issuer)
                .Build()
                .Sign(principleSignature, _context)
                .Return();

            BlockChain blockChain = await new BlockChainBuilder()
                .SetDocumentId(documentId)
                .SetPrincipleId(issuer)
                .Build(principleSignature, _context)
                .Return();

            blockChain.Add(data);

            Option result = await blockChain.ValidateBlockChain(principleSignature, _context);
            result.StatusCode.IsOk().BeTrue();

            // Get payload of data block
            blockChain.Count.Be(2);

            var blocksOption = blockChain.Filter(issuer);
            blocksOption.IsOk().BeTrue();
            IReadOnlyList<DataBlock> blocks = blocksOption.Return().ToArray();

            DataBlock genesis = blocks[0];
            var genesisBlock = new GenesisBlock
            {
                DocumentId = documentId,
                OwnerPrincipalId = issuer,
            };

            genesis.BlockType.Be(genesisBlock.Type);
            genesis.ClassType.Be(typeof(GenesisBlock).GetTypeName());
            genesis.Data.Be(genesisBlock.ToJson());
            genesis.PrincipleId.Be(issuer);

            DataBlock receiveBlock = blocks[1];
            receiveBlock.BlockType.Be("blockType");
            receiveBlock.ClassType.Be("blockClass");
            receiveBlock.Data.NotEmpty();
            receiveBlock.BlockId.Be("blockId");

            Dictionary<string, string> receivedPayload = receiveBlock.Data.ToObject<Dictionary<string, string>>().NotNull(name: "payload failed");
            (receivedPayload["name"] == "Name").BeTrue();
            (receivedPayload["type"] == "Type").BeTrue();
            (receivedPayload["author"] == "Author").BeTrue();
            (receivedPayload["data"] == "Data").BeTrue();
        }

        [Fact]
        public async Task TwoTypes()
        {
            (Payload payload, Payload2 payload2, BlockChain blockChain) = await Create();
            VerifyBlockChain(blockChain, payload, payload2);
        }

        private async Task<(Payload payload, Payload2 payload2, BlockChain blockChain)> Create()
        {
            const string issuer = "user@domain.com";
            const string issuer2 = "user2@domain.com";
            const string documentId = "contract:domain.com/contract1";
            var now = UnixDate.UtcNow;
            var date = DateTime.UtcNow;

            var signCollection = new PrincipalSignatureCollection()
                .Add(new PrincipalSignature(issuer, issuer, "userBusiness@domain.com"))
                .Add(new PrincipalSignature(issuer2, issuer2, "userBusiness2@domain.com"));

            BlockChain blockChain = await new BlockChainBuilder()
                .SetDocumentId(documentId)
                .SetPrincipleId(issuer)
                .AddAccess(new AccessBlock { Grant = BlockGrant.Write, BlockType = typeof(Payload2).GetTypeName(), PrincipalId = issuer2 })
                .Build(signCollection, _context)
                .Return();

            var payload = new Payload { Name = "Name1", Value = 2, Price = 10.5f };
            var payloadBlock = await payload
                .ToDataBlock(issuer, "objectClass")
                .Sign(signCollection, _context)
                .Return();

            var payload2 = new Payload2 { Last = "Last", Current = date, Author = "test" };
            var payloadBlock2 = await payload2
                .ToDataBlock(issuer2)
                .Sign(signCollection, _context)
                .Return();

            blockChain.Add(payloadBlock).ThrowOnError();
            blockChain.Add(payloadBlock2).ThrowOnError();

            var validationResult = await blockChain.ValidateBlockChain(signCollection, _context);
            validationResult.StatusCode.IsOk().BeTrue();

            return (payload, payload2, blockChain);
        }

        private void VerifyBlockChain(BlockChain blockChain, Payload payload, Payload2 payload2)
        {
            const string issuer = "user@domain.com";

            // Get payload of data block
            blockChain.Count.Be(4);

            var blocks = blockChain.Filter(issuer).Return().ToList();

            DataBlock receiveBlock = blocks[2];
            receiveBlock.Validate().ThrowOnError();
            receiveBlock.BlockType.Be("objectClass");
            receiveBlock.ClassType.Be("Payload");
            string payloadJson = payload.ToJson();
            receiveBlock.Data.Be(payloadJson);

            Payload p1 = receiveBlock.Data.ToObject<Payload>().NotNull(name: "payload failed");
            (payload == p1).BeTrue();

            DataBlock receiveBlock2 = blocks[3];
            receiveBlock2.Validate().ThrowOnError();
            receiveBlock2.BlockType.Be("Payload2");
            receiveBlock2.ClassType.Be("Payload2");
            string payloadJson2 = payload2.ToJson();
            receiveBlock2.Data.Be(payloadJson2);

            Payload2 p2 = receiveBlock2.Data.ToObject<Payload2>().NotNull(name: "payload2 failed");
            (payload2 == p2).BeTrue();
        }

        private record Payload
        {
            public string? Name { get; set; }
            public int Value { get; set; }
            public float Price { get; set; }
        }

        private record Payload2
        {
            public string? Last { get; set; }
            public DateTime Current { get; set; }
            public string? Author { get; set; }
        }
    }
}