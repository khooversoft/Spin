using Toolbox.Block;

namespace SpinCluster.sdk.Serialization;

public struct DataBlockSerialization
{
    public string BlockId;
    public long TimeStamp;
    public string BlockType;
    public string ClassType;
    public string Data;
    public string PrincipleId;
    public string JwtSignature;
    public string Digest;
}


[RegisterConverter]
public sealed class DataBlockSerializationConverter : IConverter<DataBlock, DataBlockSerialization>
{
    public DataBlock ConvertFromSurrogate(in DataBlockSerialization surrogate) => new DataBlock
    {
        BlockId = surrogate.BlockId,
        TimeStamp = surrogate.TimeStamp,
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
        TimeStamp = value.TimeStamp,
        BlockType = value.BlockType,
        ClassType = value.ClassType,
        Data = value.Data,
        PrincipleId = value.PrincipleId,
        JwtSignature = value.JwtSignature,
        Digest = value.Digest,
    };
}

