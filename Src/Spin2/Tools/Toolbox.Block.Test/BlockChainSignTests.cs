using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Toolbox.Block;
using Toolbox.Data;
using Toolbox.Extensions;
using Toolbox.Security.Principal;
using Toolbox.Tools;
using Toolbox.Tools.Validation;
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
                .SetObjectId("user/tenant/user@domain.com")
                .SetPrincipleId(issuer)
                .Build(principleSignature, _context)
                .Return();

            Option result = await blockChain.ValidateBlockChain(principleSignature, _context);
            result.StatusCode.IsOk().Should().BeTrue();

            blockChain.GetDigest().Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task AppendSingleNode()
        {
            const string issuer = "user@domain.com";
            const string objectId = $"user/tenant/{issuer}";

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
                .SetObjectId(objectId)
                .SetPrincipleId(issuer)
                .Build(principleSignature, _context)
                .Return();

            blockChain.Add(data);

            Option result = await blockChain.ValidateBlockChain(principleSignature, _context);
            result.StatusCode.IsOk().Should().BeTrue();

            // Get payload of data block
            blockChain.Count.Should().Be(2);

            var nodes = blockChain.GetNodeReader(issuer).Return();

            DataBlock genesis = nodes[0].DataBlock;
            var genesisBlock = new GenesisBlock
            {
                ObjectId = objectId,
                OwnerPrincipalId = issuer,
            };

            genesis.BlockType.Should().Be(genesisBlock.Type);
            genesis.ClassType.Should().Be(typeof(GenesisBlock).GetTypeName());
            genesis.Data.Should().Be(genesisBlock.ToJson());
            genesis.PrincipleId.Should().Be(issuer);

            DataBlock receiveBlock = nodes[1].DataBlock;
            receiveBlock.BlockType.Should().Be("blockType");
            receiveBlock.ClassType.Should().Be("blockClass");
            receiveBlock.Data.Should().NotBeNullOrEmpty();
            receiveBlock.BlockId.Should().Be("blockId");

            Dictionary<string, string> receivedPayload = receiveBlock.Data.ToObject<Dictionary<string, string>>().NotNull(name: "payload failed");
            (receivedPayload["name"] == "Name").Should().BeTrue();
            (receivedPayload["type"] == "Type").Should().BeTrue();
            (receivedPayload["author"] == "Author").Should().BeTrue();
            (receivedPayload["data"] == "Data").Should().BeTrue();
        }

        [Fact]
        public async Task TwoTypes()
        {
            (Payload payload, Payload2 payload2, BlockChain blockChain) = await Create();
            VerifyBlockChain(blockChain, payload, payload2);
        }

        [Fact]
        public async Task BlockChainSerialization()
        {
            (Payload payload, Payload2 payload2, BlockChain blockChain) = await Create();
            string blockChainDigest = blockChain.GetDigest();

            BlobPackage blobPackage = blockChain.ToBlobPackage();
            blobPackage.Validate().ThrowOnError();

            BlockChain readBlockChain = blobPackage.ToBlockChain(_context).Return();
            string readBlockChainDigest = readBlockChain.GetDigest();

            VerifyBlockChain(readBlockChain, payload, payload2);
            (blockChainDigest == readBlockChainDigest).Should().BeTrue();
        }

        private async Task<(Payload payload, Payload2 payload2, BlockChain blockChain)> Create()
        {
            const string issuer = "user@domain.com";
            const string issuer2 = "user2@domain.com";
            const string objectId = $"user/tenant/{issuer}";
            var now = UnixDate.UtcNow;
            var date = DateTime.UtcNow;

            var signCollection = new PrincipalSignatureCollection()
                .Add(new PrincipalSignature(issuer, issuer, "userBusiness@domain.com"))
                .Add(new PrincipalSignature(issuer2, issuer2, "userBusiness2@domain.com"));

            BlockChain blockChain = await new BlockChainBuilder()
                .SetObjectId(objectId)
                .SetPrincipleId(issuer)
                .AddAccess(new BlockAccess { Grant = BlockGrant.Write, BlockType = typeof(Payload2).GetTypeName(), PrincipalId = issuer2 })
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
            validationResult.StatusCode.IsOk().Should().BeTrue();

            return (payload, payload2, blockChain);
        }

        private void VerifyBlockChain(BlockChain blockChain, Payload payload, Payload2 payload2)
        {
            const string issuer = "user@domain.com";

            // Get payload of data block
            blockChain.Count.Should().Be(4);

            var nodes = blockChain.GetNodeReader(issuer).Return();

            DataBlock receiveBlock = nodes[2].DataBlock;
            receiveBlock.Validate().ThrowOnError();
            receiveBlock.BlockType.Should().Be("objectClass");
            receiveBlock.ClassType.Should().Be("Payload");
            string payloadJson = payload.ToJson();
            receiveBlock.Data.Should().Be(payloadJson);

            Payload p1 = receiveBlock.Data.ToObject<Payload>().NotNull(name: "payload failed");
            (payload == p1).Should().BeTrue();

            DataBlock receiveBlock2 = nodes[3].DataBlock;
            receiveBlock2.Validate().ThrowOnError();
            receiveBlock2.BlockType.Should().Be("Payload2");
            receiveBlock2.ClassType.Should().Be("Payload2");
            string payloadJson2 = payload2.ToJson();
            receiveBlock2.Data.Should().Be(payloadJson2);

            Payload2 p2 = receiveBlock2.Data.ToObject<Payload2>().NotNull(name: "payload2 failed");
            (payload2 == p2).Should().BeTrue();
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