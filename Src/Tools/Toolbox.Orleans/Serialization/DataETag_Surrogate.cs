using System.Collections.Immutable;
using Toolbox.Types;

namespace Toolbox.Orleans;

[GenerateSerializer]
public struct DataETag_Surrogate
{
    [Id(0)] public byte[]? Data;
    [Id(1)] public string? ETag;
}


[RegisterConverter]
public sealed class DataETag_SurrogateConverter : IConverter<DataETag, DataETag_Surrogate>
{
    public DataETag ConvertFromSurrogate(in DataETag_Surrogate surrogate)
    {
        ImmutableArray<byte> data = surrogate.Data != null ? surrogate.Data.ToImmutableArray() : ImmutableArray<byte>.Empty;
        return new DataETag(data, surrogate.ETag);
    }

    public DataETag_Surrogate ConvertToSurrogate(in DataETag value) => new DataETag_Surrogate
    {
        Data = value.Data.IsDefault ? null : value.Data.ToArray(),
        ETag = value.ETag,
    };
}