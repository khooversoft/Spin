using Toolbox.Block;

namespace SpinCluster.sdk.Serialization;

[GenerateSerializer]
public struct DataBlock_Surrogate
{
    [Id(0)] public string BlockId;
    [Id(1)] public DateTime CreatedDate;
    [Id(2)] public string BlockType;
    [Id(3)] public string ClassType;
    [Id(4)] public string Data;
    [Id(5)] public string PrincipleId;
    [Id(6)] public string JwtSignature;
    [Id(7)] public string Digest;
}


[RegisterConverter]
public sealed class DataBlock_SurrogateConverter : IConverter<DataBlock, DataBlock_Surrogate>
{
    public DataBlock ConvertFromSurrogate(in DataBlock_Surrogate surrogate) => new DataBlock
    {
        BlockId = surrogate.BlockId,
        CreatedDate = surrogate.CreatedDate,
        BlockType = surrogate.BlockType,
        ClassType = surrogate.ClassType,
        Data = surrogate.Data,
        PrincipleId = surrogate.PrincipleId,
        JwtSignature = surrogate.JwtSignature,
        Digest = surrogate.Digest,
    };

    public DataBlock_Surrogate ConvertToSurrogate(in DataBlock value) => new DataBlock_Surrogate
    {
        BlockId = value.BlockId,
        CreatedDate = value.CreatedDate,
        BlockType = value.BlockType,
        ClassType = value.ClassType,
        Data = value.Data,
        PrincipleId = value.PrincipleId,
        JwtSignature = value.JwtSignature,
        Digest = value.Digest,
    };
}

