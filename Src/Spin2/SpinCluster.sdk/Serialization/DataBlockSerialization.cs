using Toolbox.Block;

namespace SpinCluster.sdk.Serialization;

[GenerateSerializer]
public struct DataBlockSerialization
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
public sealed class DataBlockSerializationConverter : IConverter<DataBlock, DataBlockSerialization>
{
    public DataBlock ConvertFromSurrogate(in DataBlockSerialization surrogate) => new DataBlock
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

    public DataBlockSerialization ConvertToSurrogate(in DataBlock value) => new DataBlockSerialization
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

