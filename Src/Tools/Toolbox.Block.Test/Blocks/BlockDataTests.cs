using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Toolbox.Extensions;
using Toolbox.Security.Sign;
using Toolbox.Tools;
using Toolbox.Types;
using Xunit;

namespace Toolbox.Block.Test.Blocks
{
    public class BlockDataTests
    {
        [Fact]
        public async Task GivenBlockData_WhenValuesSet_VerifyNoChangeAndSignature()
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

            DataBlock data = await new DataBlockBuilder()
                .SetTimeStamp(now)
                .SetBlockType("blockType")
                .SetBlockId("blockId")
                .SetData(payloadJson)
                .SetPrincipleSignature(principleSignature)
                .Build();

            data.Validate(principleSignature);

            string json = data.ToJson();

            DataBlock received = Json.Default.Deserialize<DataBlock>(json).VerifyNotNull("Json is null");

            data.TimeStamp.Should().Be(now.ToUnixDate().TimeStamp);

            received.BlockType.Should().Be("blockType");
            received.BlockId.Should().Be("blockId");
            received.Data.Should().Be(payloadJson);
            received.JwtSignature.Should().Be(data.JwtSignature);
            received.Properties.Should().NotBeNull();
            received.Properties.Count.Should().Be(0);
            received.Digest.Should().Be(received.GetDigest());

            Dictionary<string, string>? readData = Json.Default.Deserialize<Dictionary<string, string>>(data.Data)!;

            readData["name"].Should().Be(dataPayload.Name);
            readData["type"].Should().Be(dataPayload.Type);
            readData["author"].Should().Be(dataPayload.Author);
            readData["data"].Should().Be(dataPayload.Data);
        }

        [Fact]
        public async Task GivenBlockData_WhenTestForEqual_ShouldPass()
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

            DataBlock data1 = await new DataBlockBuilder()
                .SetTimeStamp(now)
                .SetBlockType("blockType")
                .SetBlockId("blockId")
                .SetData(payloadJson)
                .SetPrincipleSignature(principleSignature)
                .Build();

            DataBlock data2 = await new DataBlockBuilder()
                .SetTimeStamp(now)
                .SetBlockType("blockType")
                .SetBlockId("blockId")
                .SetData(payloadJson)
                .SetPrincipleSignature(principleSignature)
                .Build();

            data1.TimeStamp.Should().Be(data2.TimeStamp);
            data1.BlockType.Should().Be(data2.BlockType);
            data1.BlockId.Should().Be(data2.BlockId);
            data1.Data.Should().Be(data2.Data);
            data1.Digest.Should().Be(data2.Digest);

            (data1 == data2).Should().BeTrue();        // Will fail because of timestamp on JwtSignature
        }
    }
}