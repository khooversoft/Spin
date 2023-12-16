using Toolbox.Data;

namespace SpinCluster.sdk.Serialization;

[GenerateSerializer]
public struct StorageBlob_Surrogate
{
    [Id(0)] public string StorageId;
    [Id(1)] public byte[] Content;
    [Id(2)] public string? ETag;
    [Id(3)] public string BlobHash;
}


[RegisterConverter]
public sealed class StorageBlob_SurrogateConverter : IConverter<StorageBlob, StorageBlob_Surrogate>
{
    public StorageBlob ConvertFromSurrogate(in StorageBlob_Surrogate surrogate) => new StorageBlob
    {
        StorageId = surrogate.StorageId,
        Content = surrogate.Content,
        ETag = surrogate.ETag,
        BlobHash = surrogate.BlobHash,
    };

    public StorageBlob_Surrogate ConvertToSurrogate(in StorageBlob value) => new StorageBlob_Surrogate
    {
        StorageId = value.StorageId,
        Content = value.Content,
        ETag = value.ETag,
        BlobHash = value.BlobHash,
    };
}