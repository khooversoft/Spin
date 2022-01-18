using FluentAssertions;
using System;
using System.Collections.Generic;
using Toolbox.Extensions;
using Toolbox.Security;
using Toolbox.Security.Extensions;
using Toolbox.Security.Sign;
using Toolbox.Tools;
using Toolbox.Types;
using Xunit;

namespace Toolbox.Block.Test.Blocks
{
    public class BlockChainSignTests
    {
        [Fact]
        public void GivenEmptyBlockChain_ShouldVerify()
        {
            const string issuer = "user@domain.com";
            var now = UnixDate.UtcNow;

            IPrincipleSignature principleSignature = new PrincipleSignature(issuer, issuer, "userBusiness@domain.com");

            BlockChain blockChain = new BlockChainBuilder()
                .SetPrincipleSignature(principleSignature)
                .Build();

            blockChain.Validate(x => principleSignature);
        }

        [Fact]
        public void GivenBlockChain_AppendSingleNode_ShouldVerify()
        {
            const string issuer = "user@domain.com";
            var now = UnixDate.UtcNow;

            IPrincipleSignature principleSignature = new PrincipleSignature(issuer, issuer, "userBusiness@domain.com");

            var dataPayload = new
            {
                Name = "Name",
                Type = "Type",
                Author = "Author",
                Data = "Data"
            };

            string payloadJson = dataPayload.ToJson();

            DataBlock data = new DataBlockBuilder()
                .SetTimeStamp(now)
                .SetBlockType("blockType")
                .SetBlockId("blockId")
                .SetData(payloadJson)
                .SetPrincipleSignature(principleSignature)
                .Build();

            BlockChain blockChain = new BlockChainBuilder()
                .SetPrincipleSignature(principleSignature)
                .Build()
                .Add(data);

            blockChain.Validate(x => principleSignature);

            // Get payload of data block
            blockChain.Blocks.Count.Should().Be(2);

            DataBlock receiveBlock = blockChain.Blocks[1].BlockData;
            TestBlockNode(receiveBlock, "blockType", "blockId");

            Dictionary<string, string> receivedPayload = receiveBlock.Data.ToObject<Dictionary<string, string>>().VerifyNotNull("payload failed");
            (receivedPayload["name"] == "Name").Should().BeTrue();
            (receivedPayload["type"] == "Type").Should().BeTrue();
            (receivedPayload["author"] == "Author").Should().BeTrue();
            (receivedPayload["data"] == "Data").Should().BeTrue();
        }

        [Fact]
        public void GivenBlockChain_TwoTypes_ShouldVerify()
        {
            const string issuer = "user@domain.com";
            const string issuer2 = "user2@domain.com";
            var now = UnixDate.UtcNow;
            var date = DateTime.UtcNow;

            var issuerSignature = new PrincipleSignature(issuer, issuer, "userBusiness@domain.com");
            var issuerSignature2 = new PrincipleSignature(issuer2, issuer2, "userBusiness2@domain.com");

            BlockChain blockChain = new BlockChainBuilder()
                .SetPrincipleSignature(issuerSignature)
                .Build();

            var payload = new Payload { Name = "Name1", Value = 2, Price = 10.5f };
            var payload2 = new Payload2 { Last = "Last", Current = date, Author = "test" };

            blockChain.Add(payload, issuerSignature);
            blockChain.Add(payload2, issuerSignature2);

            blockChain.Validate(x =>
            {
                string kid = JwtTokenParser.GetKidFromJwtToken(x).VerifyNotEmpty(nameof(kid));

                return kid switch
                {
                    issuer => issuerSignature,
                    issuer2 => issuerSignature2,
                    _ => throw new ArgumentException("Invalid kid"),
                };
            });

            // Get payload of data block
            blockChain.Blocks.Count.Should().Be(3);

            DataBlock receiveBlock = blockChain.Blocks[1].BlockData;
            TestBlockNode(receiveBlock, "Payload", "1");

            Payload p1 = receiveBlock.Data.ToObject<Payload>().VerifyNotNull("payload failed");
            (payload == p1).Should().BeTrue();

            DataBlock receiveBlock2 = blockChain.Blocks[2].BlockData;
            TestBlockNode(receiveBlock2, "Payload2", "2");

            Payload2 p2 = receiveBlock2.Data.ToObject<Payload2>().VerifyNotNull("payload2 failed");
            (payload2 == p2).Should().BeTrue();
        }

        [Fact]
        public void GivenBlockChain_ShouldSerializeAndDeserialize()
        {
            const string issuer = "user@domain.com";
            const string issuer2 = "user2@domain.com";
            var now = UnixDate.UtcNow;
            var date = DateTime.UtcNow;

            var issuerSignature = new PrincipleSignature(issuer, issuer, "userBusiness@domain.com");
            var issuerSignature2 = new PrincipleSignature(issuer2, issuer2, "userBusiness2@domain.com");

            BlockChain blockChain = new BlockChainBuilder()
                .SetPrincipleSignature(issuerSignature)
                .Build();

            var payload = new Payload { Name = "Name1", Value = 2, Price = 10.5f };
            var payload2 = new Payload2 { Last = "Last", Current = date, Author = "test" };

            blockChain.Add(payload, issuerSignature);
            blockChain.Add(payload2, issuerSignature2);

            var getSignature = (string x) =>
            {
                string kid = JwtTokenParser.GetKidFromJwtToken(x).VerifyNotEmpty(nameof(kid));

                return kid switch
                {
                    issuer => issuerSignature,
                    issuer2 => issuerSignature2,
                    _ => throw new ArgumentException("Invalid kid"),
                };
            };

            blockChain.Validate(getSignature);

            string blockChainJson = blockChain.ToJson();

            // Receive test
            BlockChain receivedChain = blockChainJson.ToObject<BlockChainModel>()
                .VerifyNotNull("Cannot deserialize")
                .ConvertTo();

            receivedChain.Should().NotBeNull();
            receivedChain.Validate(getSignature);

            receivedChain.Blocks.Count.Should().Be(3);

            DataBlock receiveBlock = receivedChain.Blocks[1].BlockData;
            TestBlockNode(receiveBlock, "Payload", "1");

            Payload p1 = receiveBlock.Data.ToObject<Payload>().VerifyNotNull("payload failed");
            (payload == p1).Should().BeTrue();

            DataBlock receiveBlock2 = receivedChain.Blocks[2].BlockData;
            TestBlockNode(receiveBlock2, "Payload2", "2");

            Payload2 p2 = receiveBlock2.Data.ToObject<Payload2>().VerifyNotNull("payload2 failed");
            (payload2 == p2).Should().BeTrue();
        }

        private void TestBlockNode(DataBlock dataBlock, string blockType, string blockId)
        {
            dataBlock.BlockType.Should().Be(blockType);
            dataBlock.BlockId.Should().Be(blockId);
            dataBlock.Properties.Should().NotBeNull();
            dataBlock.Properties.Count.Should().Be(0);
            dataBlock.Data.Should().NotBeNullOrEmpty();
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