using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Toolbox.Extensions;
using Toolbox.Security.Sign;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Block;

public class DataBlockBuilder
{
    public DateTime TimeStamp { get; set; } = DateTime.UtcNow;

    public string? BlockType { get; set; }

    public string? BlockId { get; set; }

    public string? Data { get; set; }

    public Dictionary<string, string> Properties { get; set; } = new Dictionary<string, string>();

    public Func<string, Task<string>>? Sign { get; set; }

    public DataBlockBuilder SetTimeStamp(DateTime timestamp) => this.Action(x => x.TimeStamp = timestamp);

    public DataBlockBuilder SetBlockType(string blockType) => this.Action(x => x.BlockType = blockType);

    public DataBlockBuilder SetBlockId(string blockId) => this.Action(x => x.BlockId = blockId);

    public DataBlockBuilder SetData(string data) => this.Action(x => Data = data);

    public DataBlockBuilder SetSign(Func<string, Task<string>> sign) => this.Action(x => Sign = sign);

    public async Task<DataBlock> Build()
    {
        BlockType.VerifyNotEmpty($"{nameof(BlockType)} is required");
        BlockId.VerifyNotEmpty($"{nameof(BlockId)} is required");
        Data.VerifyNotEmpty($"{nameof(Data)} is required");
        Sign.VerifyNotNull($"{nameof(Sign)} is required");

        Properties ??= new Dictionary<string, string>();

        DataBlock dataBlock = new DataBlock
        {
            TimeStamp = TimeStamp.ToUnixDate(),
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
            JwtSignature = await Sign(digest)
        };
    }

    public static Task<DataBlock> CreateGenesisBlock(Func<string, Task<string>> sign) => new DataBlockBuilder()
        .SetBlockType("genesis")
        .SetBlockId("0")
        .SetData("genesis")
        .SetSign(sign)
        .Build();
}


public static class DataBlockBuilderExtensions
{
    public static DataBlockBuilder SetPrincipleSignature(this DataBlockBuilder subject, IPrincipalSignature principleSignature)
    {
        subject.VerifyNotNull(nameof(subject));
        principleSignature.VerifyNotNull(nameof(principleSignature));

        subject.SetSign(x => Task.FromResult(principleSignature.Sign(x)));

        return subject;
    }
}
