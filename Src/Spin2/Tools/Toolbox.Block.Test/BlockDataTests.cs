using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Toolbox.Extensions;
using Toolbox.Security.Principal;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Block.Test
{
    public class BlockDataTests
    {
        private readonly ScopeContext _context = new ScopeContext(NullLogger.Instance);

        [Fact]
        public async Task BlockCreateWithPayloadAndSerialized()
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
                .SetBlockId("blockId")
                .SetBlockType("blockType")
                .SetObjectClass("blockClass")
                .SetData(payloadJson)
                .SetPrincipleId(issuer)
                .Build()
                .Sign(principleSignature, _context)
                .Return();

            await data.ValidateDigest(principleSignature, _context);

            string json = data.ToJson();

            DataBlock received = Json.Default.Deserialize<DataBlock>(json).NotNull(name: "Json is null");
            await received.ValidateDigest(principleSignature, _context);

            data.TimeStamp.Should().Be(now.ToUnixDate().TimeStamp);

            received.BlockType.Should().Be("blockType");
            received.ClassType.Should().Be("blockClass");
            received.BlockId.Should().Be("blockId");
            received.Data.Should().Be(payloadJson);
            received.JwtSignature.Should().Be(data.JwtSignature);
            received.Digest.Should().Be(received.CalculateDigest());

            Dictionary<string, string>? readData = Json.Default.Deserialize<Dictionary<string, string>>(data.Data)!;

            readData["name"].Should().Be(dataPayload.Name);
            readData["type"].Should().Be(dataPayload.Type);
            readData["author"].Should().Be(dataPayload.Author);
            readData["data"].Should().Be(dataPayload.Data);
        }

        [Fact]
        public async Task TwoIdenticalBlockCreatedAreEqual()
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
                .SetBlockId("blockId")
                .SetBlockType("blockType")
                .SetObjectClass("blockClass")
                .SetData(payloadJson)
                .SetPrincipleId(issuer)
                .Build()
                .Sign(principleSignature, _context)
                .Return();

            DataBlock data2 = await new DataBlockBuilder()
                .SetTimeStamp(now)
                .SetBlockId("blockId")
                .SetBlockType("blockType")
                .SetObjectClass("blockClass")
                .SetData(payloadJson)
                .SetPrincipleId(issuer)
                .Build()
                .Sign(principleSignature, _context)
                .Return();

            data1.TimeStamp.Should().Be(data2.TimeStamp);
            data1.BlockType.Should().Be(data2.BlockType);
            data1.ClassType.Should().Be(data2.ClassType);
            data1.Data.Should().Be(data2.Data);
            //data1.JwtSignature.Should().Be(data2.JwtSignature);
            data1.Digest.Should().Be(data2.Digest);

            //(data1 == data2).Should().BeTrue();
        }
    }
}