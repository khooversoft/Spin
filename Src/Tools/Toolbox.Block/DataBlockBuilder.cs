using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Toolbox.Extensions;
using Toolbox.Monads;
using Toolbox.Security.Sign;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Block;

public class DataBlockBuilder
{
    public DateTime TimeStamp { get; set; } = DateTime.UtcNow;
    public string? BlockType { get; set; }
    public string? ObjectClass { get; set; }
    public string BlockId { get; set; } = Guid.NewGuid().ToString();
    public string? Data { get; set; }
    public string? PrincipleId { get; set; }

    public DataBlockBuilder SetTimeStamp(DateTime timestamp) => this.Action(x => x.TimeStamp = timestamp);
    public DataBlockBuilder SetBlockType(string blockType) => this.Action(x => x.BlockType = blockType);
    public DataBlockBuilder SetObjectClass(string objectClass) => this.Action(x => x.ObjectClass = objectClass);
    public DataBlockBuilder SetBlockType<T>() => this.Action(x => x.BlockType = typeof(T).Name);
    public DataBlockBuilder SetBlockId(string blockId) => this.Action(x => x.BlockId = blockId);
    public DataBlockBuilder SetData(string data) => this.Action(x => Data = data);
    public DataBlockBuilder SetPrincipleId(string principleId) => this.Action(x => PrincipleId = principleId);

    public DataBlockBuilder SetData<T>(T data) where T : class
    {
        data.NotNull();

        ObjectClass = data.GetType().Name;
        Data = data?.ToJson();
        return this;
    }

    public DataBlock Build()
    {
        BlockType.NotEmpty(name: $"{nameof(BlockType)} is required");
        ObjectClass.NotEmpty(name: $"{nameof(ObjectClass)} is required");
        BlockId.NotEmpty(name: $"{nameof(BlockId)} is required");
        Data.NotEmpty(name: $"{nameof(Data)} is required");
        PrincipleId.NotEmpty(name: $"{nameof(SetPrincipleId)} is required");

        DataBlock dataBlock = new DataBlock
        {
            TimeStamp = TimeStamp.ToUnixDate(),
            BlockType = BlockType,
            ObjectClass = ObjectClass,
            BlockId = BlockId,
            Data = Data,
            PrincipleId = PrincipleId,
        };

        return dataBlock with { Digest = dataBlock.GetDigest() };
    }

    public static DataBlock CreateGenesisBlock(string principleId) => "genesis"
        .Func(x => new DataBlockBuilder()
            .SetBlockType(x)
            .SetObjectClass(x)
            .SetData(x)
            .SetPrincipleId(principleId)
            .Build()
        );
}
