using System.Collections.Generic;
using System.Linq;
using Toolbox.Extensions;
using Toolbox.Security;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.BlockDocument.Block
{
    public class DataBlockBuilder
    {
        public UnixDate TimeStamp { get; set; } = UnixDate.UtcNow;

        public string? BlockType { get; set; }

        public string? BlockId { get; set; }

        public string? Data { get; set; }

        public Dictionary<string, string> Properties { get; set; } = new Dictionary<string, string>();

        public IPrincipleSignature? PrincipleSignature { get; set; }

        public DataBlockBuilder SetTimeStamp(UnixDate unixDate) => this.Action(x => x.TimeStamp = unixDate);

        public DataBlockBuilder SetBlockType(string blockType) => this.Action(x => x.BlockType = blockType);

        public DataBlockBuilder SetBlockId(string blockId) => this.Action(x => x.BlockId = blockId);

        public DataBlockBuilder SetData(string data) => this.Action(x => Data = data);

        public DataBlockBuilder SetPrincipleSignature(IPrincipleSignature principleSignature) => this.Action(x => PrincipleSignature = principleSignature);

        public DataBlock Build()
        {
            BlockType.VerifyNotEmpty($"{nameof(BlockType)} is required");
            BlockId.VerifyNotEmpty($"{nameof(BlockId)} is required");
            Data.VerifyNotEmpty($"{nameof(Data)} is required");
            PrincipleSignature.VerifyNotNull($"{nameof(PrincipleSignature)} is required");

            Properties ??= new Dictionary<string, string>();

            DataBlock dataBlock = new DataBlock
            {
                TimeStamp = TimeStamp,
                BlockType = BlockType,
                BlockId = BlockId,
                Data = Data,
                Properties = Properties.ToDictionary(x => x.Key, x => x.Value),
            };

            string digest = dataBlock.GetDigest();

            return new DataBlock
            {
                TimeStamp = dataBlock.TimeStamp,
                BlockType = dataBlock.BlockType,
                BlockId = dataBlock.BlockId,
                Data = dataBlock.Data,
                Properties = dataBlock.Properties,

                Digest = digest,
                JwtSignature = PrincipleSignature.Sign(digest)
            };
        }

        public static DataBlock CreateGenesisBlock(IPrincipleSignature principleSignature) => new DataBlockBuilder()
            .SetBlockType("genesis")
            .SetBlockId("0")
            .SetData("genesis")
            .SetPrincipleSignature(principleSignature)
            .Build();
    }
}
