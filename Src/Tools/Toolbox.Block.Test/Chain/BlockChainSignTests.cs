using System;
using System.Collections.Generic;
using FluentAssertions;
using Toolbox.Abstractions.Extensions;
using Toolbox.Abstractions.Tools;
using Toolbox.Block.Application;
using Toolbox.Block.Container;
using Toolbox.Block.Serialization;
using Toolbox.Block.Signature;
using Toolbox.Extensions;
using Toolbox.Security.Sign;
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

            IPrincipalSignature principleSignature = new PrincipalSignature(issuer, issuer, "userBusiness@domain.com");

            BlockChain blockChain = new BlockChainBuilder()
                .SetPrincipleId(issuer)
                .Build()
                .Sign(x => principleSignature);

            blockChain.Validate(x => principleSignature);
        }

        [Fact]
        public void GivenBlockChain_AppendSingleNode_ShouldVerify()
        {
            const string issuer = "user@domain.com";
            var now = DateTime.UtcNow;

            IPrincipalSignature principleSignature = new PrincipalSignature(issuer, issuer, "userBusiness@domain.com");

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
                .SetBlockId("blockId")
                .SetBlockType("blockType")
                .SetObjectClass("blockClass")
                .SetData(payloadJson)
                .SetPrincipleId(issuer)
                .Build();

            BlockChain blockChain = new BlockChainBuilder()
                .SetPrincipleId(issuer)
                .Build()
                .Add(data)
                .Sign(x => principleSignature);

            blockChain.Validate(x => principleSignature);

            // Get payload of data block
            blockChain.Blocks.Count.Should().Be(2);

            DataBlock receiveBlock = blockChain.Blocks[1].DataBlock;
            TestBlockNode(receiveBlock, "blockType", "blockId", "blockClass");

            Dictionary<string, string> receivedPayload = receiveBlock.Data.ToObject<Dictionary<string, string>>().NotNull(name: "payload failed");
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

            var issuerSignature = new PrincipalSignature(issuer, issuer, "userBusiness@domain.com");
            var issuerSignature2 = new PrincipalSignature(issuer2, issuer2, "userBusiness2@domain.com");

            BlockChain blockChain = new BlockChainBuilder()
                .SetPrincipleId(issuer)
                .Build();

            var payload = new Payload { Name = "Name1", Value = 2, Price = 10.5f };
            var payload2 = new Payload2 { Last = "Last", Current = date, Author = "test" };

            blockChain.Add(payload, issuer, "objectClass");
            blockChain.Add(payload2, issuer2, "objectClass");

            var getSignature = (string kid) => kid switch
            {
                issuer => issuerSignature,
                issuer2 => issuerSignature2,
                _ => throw new ArgumentException($"Invalid kid={kid}"),
            };

            blockChain = blockChain.Sign(getSignature);

            blockChain.Validate(getSignature);

            // Get payload of data block
            blockChain.Blocks.Count.Should().Be(3);

            DataBlock receiveBlock = blockChain.Blocks[1].DataBlock;
            TestBlockNode(receiveBlock, "objectClass", null, "Payload");

            Payload p1 = receiveBlock.Data.ToObject<Payload>().NotNull(name: "payload failed");
            (payload == p1).Should().BeTrue();

            DataBlock receiveBlock2 = blockChain.Blocks[2].DataBlock;
            TestBlockNode(receiveBlock2, "objectClass", null, "Payload2");

            Payload2 p2 = receiveBlock2.Data.ToObject<Payload2>().NotNull(name: "payload2 failed");
            (payload2 == p2).Should().BeTrue();
        }

        [Fact]
        public void GivenBlockChain_ShouldSerializeAndDeserialize()
        {
            const string issuer = "user@domain.com";
            const string issuer2 = "user2@domain.com";
            var now = UnixDate.UtcNow;
            var date = DateTime.UtcNow;

            var issuerSignature = new PrincipalSignature(issuer, issuer, "userBusiness@domain.com");
            var issuerSignature2 = new PrincipalSignature(issuer2, issuer2, "userBusiness2@domain.com");

            BlockChain blockChain = new BlockChainBuilder()
                .SetPrincipleId(issuer)
                .Build();

            var payload = new Payload { Name = "Name1", Value = 2, Price = 10.5f };
            var payload2 = new Payload2 { Last = "Last", Current = date, Author = "test" };

            blockChain.Add(payload, issuer, "objectClass");
            blockChain.Add(payload2, issuer2, "objectClass");

            var getSignature = (string kid) => kid switch
            {
                issuer => issuerSignature,
                issuer2 => issuerSignature2,
                _ => throw new ArgumentException($"Invalid kid={kid}"),
            };

            blockChain = blockChain.Sign(getSignature);

            blockChain.Validate(getSignature);

            string blockChainJson = blockChain.ToJson();

            // Receive test
            BlockChain receivedChain = blockChainJson.ToObject<BlockChainModel>()
                .NotNull(name: "Cannot deserialize")
                .ToBlockChain();

            receivedChain.Should().NotBeNull();
            receivedChain.Validate(getSignature);

            receivedChain.Blocks.Count.Should().Be(3);

            DataBlock receiveBlock = receivedChain.Blocks[1].DataBlock;
            TestBlockNode(receiveBlock, "objectClass", null, "Payload");

            Payload p1 = receiveBlock.Data.ToObject<Payload>().NotNull(name: "payload failed");
            (payload == p1).Should().BeTrue();

            DataBlock receiveBlock2 = receivedChain.Blocks[2].DataBlock;
            TestBlockNode(receiveBlock2, "objectClass", null, "Payload2");

            Payload2 p2 = receiveBlock2.Data.ToObject<Payload2>().NotNull(name: "payload2 failed");
            (payload2 == p2).Should().BeTrue();
        }

        private void TestBlockNode(DataBlock dataBlock, string blockType, string? blockId, string objectClass)
        {
            dataBlock.BlockType.Should().Be(blockType);
            dataBlock.ObjectClass.Should().Be(objectClass);
            dataBlock.Data.Should().NotBeNullOrEmpty();

            if (!blockId.IsEmpty()) dataBlock.BlockId.Should().Be(blockId);
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