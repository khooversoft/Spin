using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;
using Toolbox.Tools.Validation;

namespace Toolbox.Block;

public class DataBlockBuilder
{
    public DateTime TimeStamp { get; set; } = DateTime.UtcNow;
    public string? BlockType { get; set; }
    public string? ClassType { get; set; }
    public string BlockId { get; set; } = Guid.NewGuid().ToString();
    public string? Data { get; set; }
    public string? PrincipleId { get; set; }

    public DataBlockBuilder SetTimeStamp(DateTime timestamp) => this.Action(x => x.TimeStamp = timestamp);
    public DataBlockBuilder SetBlockType(string blockType) => this.Action(x => x.BlockType = blockType);
    public DataBlockBuilder SetBlockType<T>() => this.Action(x => x.BlockType = typeof(T).GetTypeName());
    public DataBlockBuilder SetObjectClass(string classType) => this.Action(x => x.ClassType = classType);
    public DataBlockBuilder SetBlockId(string blockId) => this.Action(x => x.BlockId = blockId);
    public DataBlockBuilder SetData(string data) => this.Action(x => Data = data);
    public DataBlockBuilder SetPrincipleId(string principleId) => this.Action(x => PrincipleId = principleId);

    public DataBlockBuilder SetData<T>(T data) where T : class
    {
        data.NotNull();

        ClassType = data.GetType().GetTypeName();
        Data = data?.ToJson();
        return this;
    }

    public DataBlock Build()
    {
        const string msg = "is required";
        BlockType.NotEmpty(name: msg);
        ClassType.NotEmpty(name: msg);
        BlockId.NotEmpty(name: msg);
        Data.NotEmpty(name: msg);
        PrincipleId.NotEmpty(name: msg);

        var dataBlock = new DataBlock
        {
            TimeStamp = TimeStamp.ToUnixDate(),
            BlockType = BlockType,
            ClassType = ClassType,
            BlockId = BlockId,
            Data = Data,
            PrincipleId = PrincipleId,
        };

        return dataBlock with { Digest = dataBlock.CalculateDigest() };
    }

    public static DataBlock CreateGenesisBlock(string objectId, string principleId, ScopeContext context)
    {
        var marker = new GenesisBlock
        {
            ObjectId = objectId,
            OwnerPrincipalId = principleId,
        };

        GenesisBlockValidator.Validate(marker, context.Location()).ThrowOnError();

        return new DataBlockBuilder()
            .SetBlockType(GenesisBlock.BlockType)
            .SetData(marker)
            .SetPrincipleId(principleId)
            .Build();
    }
}