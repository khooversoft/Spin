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
    public DateTimeOffset TimeStamp { get; set; } = DateTimeOffset.UtcNow;

    public string? BlockType { get; set; }

    public string BlockId { get; set; } = Guid.NewGuid().ToString();

    public string? Data { get; set; }

    public string? PrincipleId { get; set; }

    public DataBlockBuilder SetTimeStamp(DateTimeOffset timestamp) => this.Action(x => x.TimeStamp = timestamp);

    public DataBlockBuilder SetBlockType(string blockType) => this.Action(x => x.BlockType = blockType);

    public DataBlockBuilder SetBlockType<T>() => this.Action(x => x.BlockType = typeof(T).Name);

    public DataBlockBuilder SetBlockId(string blockId) => this.Action(x => x.BlockId = blockId);

    public DataBlockBuilder SetData(string data) => this.Action(x => Data = data);

    public DataBlockBuilder SetData<T>(T data) => this.Action(x => Data = data?.ToJson());

    public DataBlockBuilder SetPrincipleId(string principleId) => this.Action(x => PrincipleId = principleId);

    public DataBlock Build()
    {
        BlockType.NotEmpty($"{nameof(BlockType)} is required");
        BlockId.NotEmpty($"{nameof(BlockId)} is required");
        Data.NotEmpty($"{nameof(Data)} is required");
        PrincipleId.NotEmpty($"{nameof(SetPrincipleId)} is required");

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
