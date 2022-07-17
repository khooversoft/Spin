using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Toolbox.Extensions;
using Toolbox.Security.Sign;
using Toolbox.Tools;
using Toolbox.Types;
using Toolbox.Block;
using Xunit;

namespace Toolbox.Block.Test.Blocks
{
    public class BlockDataTests
    {
        [Fact]
        public void GivenBlockData_WhenValuesSet_VerifyNoChangeAndSignature()
        {
            const string issuer = "user@domain.com";
            var now = DateTime.UtcNow;

            IPrincipalSignature principleSignature = new PrincipalSignature(issuer, issuer, "test.com");

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

            data = data with { JwtSignature = principleSignature.Sign(data.Digest) };

            principleSignature.ValidateSignature(data.JwtSignature);

            string json = data.ToJson();

            DataBlock received = Json.Default.Deserialize<DataBlock>(json).NotNull(name: "Json is null");

            data.TimeStamp.Should().Be(now.ToUnixDate().TimeStamp);

            received.BlockType.Should().Be("blockType");
            received.ObjectClass.Should().Be("blockClass");
            received.BlockId.Should().Be("blockId");
            received.Data.Should().Be(payloadJson);
            received.JwtSignature.Should().Be(data.JwtSignature);
            received.Digest.Should().Be(received.GetDigest());

            Dictionary<string, string>? readData = Json.Default.Deserialize<Dictionary<string, string>>(data.Data)!;

            readData["name"].Should().Be(dataPayload.Name);
            readData["type"].Should().Be(dataPayload.Type);
            readData["author"].Should().Be(dataPayload.Author);
            readData["data"].Should().Be(dataPayload.Data);
        }

        [Fact]
        public void GivenBlockData_WhenTestForEqual_ShouldPass()
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

            DataBlock data1 = new DataBlockBuilder()
                .SetTimeStamp(now)
                .SetBlockId("blockId")
                .SetBlockType("blockType")
                .SetObjectClass("blockClass")
                .SetData(payloadJson)
                .SetPrincipleId(issuer)
                .Build();

            DataBlock data2 = new DataBlockBuilder()
                .SetTimeStamp(now)
                .SetBlockId("blockId")
                .SetBlockType("blockType")
                .SetObjectClass("blockClass")
                .SetData(payloadJson)
                .SetPrincipleId(issuer)
                .Build();

            data1.TimeStamp.Should().Be(data2.TimeStamp);
            data1.BlockType.Should().Be(data2.BlockType);
            data1.ObjectClass.Should().Be(data2.ObjectClass);
            data1.Data.Should().Be(data2.Data);
            data1.Digest.Should().Be(data2.Digest);

            (data1 == data2).Should().BeTrue();        // Will fail because of timestamp on JwtSignature
        }
    }
}