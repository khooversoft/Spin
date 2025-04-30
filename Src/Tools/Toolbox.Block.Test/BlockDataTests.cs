using Microsoft.Extensions.Logging.Abstractions;
using Toolbox.Extensions;
using Toolbox.Security;
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

            received.BlockType.Be("blockType");
            received.ClassType.Be("blockClass");
            received.BlockId.Be("blockId");
            received.Data.Be(payloadJson);
            received.JwtSignature.Be(data.JwtSignature);
            received.Digest.Be(received.CalculateDigest());

            Dictionary<string, string>? readData = Json.Default.Deserialize<Dictionary<string, string>>(data.Data)!;

            readData["name"].Be(dataPayload.Name);
            readData["type"].Be(dataPayload.Type);
            readData["author"].Be(dataPayload.Author);
            readData["data"].Be(dataPayload.Data);
        }

        [Fact]
        public async Task TwoIdenticalBlockCreatedAreEqual()
        {
            const string issuer = "user@domain.com";
            DateTime now = DateTime.UtcNow;

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
                .SetBlockId("blockId")
                .SetBlockType("blockType")
                .SetObjectClass("blockClass")
                .SetData(payloadJson)
                .SetPrincipleId(issuer)
                .Build()
                .Func(x => x with { CreatedDate = now })
                .Func(x => x with { Digest = x.CalculateDigest() })
                .Sign(principleSignature, _context)
                .Return();

            DataBlock data2 = await new DataBlockBuilder()
                .SetBlockId("blockId")
                .SetBlockType("blockType")
                .SetObjectClass("blockClass")
                .SetData(payloadJson)
                .SetPrincipleId(issuer)
                .Build()
                .Func(x => x with { CreatedDate = now })
                .Func(x => x with { Digest = x.CalculateDigest() })
                .Sign(principleSignature, _context)
                .Return();

            data1.BlockType.Be(data2.BlockType);
            data1.ClassType.Be(data2.ClassType);
            data1.Data.Be(data2.Data);
            //data1.JwtSignature.Be(data2.JwtSignature);
            data1.Digest.Be(data2.Digest);

            //(data1 == data2).BeTrue();
        }
    }
}