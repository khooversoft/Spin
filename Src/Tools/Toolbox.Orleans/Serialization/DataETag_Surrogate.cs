using Azure;
using Toolbox.Types;

namespace Toolbox.Orleans;

[GenerateSerializer]
public struct DataETag_Surrogate
{
    public byte[] Data;
    public ETag? ETag;
}


[RegisterConverter]
public sealed class DataETag_SurrogateConverter : IConverter<DataETag, DataETag_Surrogate>
{
    public DataETag ConvertFromSurrogate(in DataETag_Surrogate surrogate) => new DataETag(surrogate.Data, surrogate.ETag);

    public DataETag_Surrogate ConvertToSurrogate(in DataETag value) => new DataETag_Surrogate
    {
        Data = value.Data,
        ETag = value.ETag,
    };
}