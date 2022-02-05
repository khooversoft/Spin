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

    public string? PrincipleId { get; set; }

    public DataBlockBuilder SetTimeStamp(DateTime timestamp) => this.Action(x => x.TimeStamp = timestamp);

    public DataBlockBuilder SetBlockType(string blockType) => this.Action(x => x.BlockType = blockType);

    public DataBlockBuilder SetBlockId(string blockId) => this.Action(x => x.BlockId = blockId);

    public DataBlockBuilder SetData(string data) => this.Action(x => Data = data);

    public DataBlockBuilder SetPrincipleId(string principleId) => this.Action(x => PrincipleId = principleId);

    public DataBlock Build()
    {
        BlockType.VerifyNotEmpty($"{nameof(BlockType)} is required");
        BlockId.VerifyNotEmpty($"{nameof(BlockId)} is required");
        Data.VerifyNotEmpty($"{nameof(Data)} is required");
        PrincipleId.VerifyNotEmpty($"{nameof(SetPrincipleId)} is required");

        DataBlock dataBlock = new DataBlock
        {
            TimeStamp = TimeStamp.ToUnixDate(),
            BlockType = BlockType,
            BlockId = BlockId,
            Data = Data,
            PrincipleId = PrincipleId,
        };

        return dataBlock with { Digest = dataBlock.GetDigest() };
    }

    public static DataBlock CreateGenesisBlock(string principleId) => new DataBlockBuilder()
        .SetBlockType("genesis")
        .SetBlockId("0")
        .SetData("genesis")
        .SetPrincipleId(principleId)
        .Build();
}
