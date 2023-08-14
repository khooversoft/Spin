using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Block;
using Toolbox.Data;

namespace SpinCluster.sdk.Serialization;

[GenerateSerializer]
public struct BlobPackageSerialization
{
    [Id(0)] public string ObjectId;
    [Id(1)] public string TypeName;
    [Id(2)] public byte[] Content;
    [Id(3)] public string? ETag;
    [Id(4)] public string? Tags;
}


[RegisterConverter]
public sealed class BlobPackageSerializationConverter : IConverter<BlobPackage, BlobPackageSerialization>
{
    public BlobPackage ConvertFromSurrogate(in BlobPackageSerialization surrogate) => new BlobPackage
    {
        ObjectId = surrogate.ObjectId,
        TypeName = surrogate.TypeName,
        Content = surrogate.Content,
        ETag = surrogate.ETag,
        Tags = surrogate.Tags,
    };

    public BlobPackageSerialization ConvertToSurrogate(in BlobPackage value) => new BlobPackageSerialization
    {
        ObjectId = value.ObjectId,
        TypeName = value.TypeName,
        Content = value.Content,
        ETag = value.ETag,
        Tags = value.Tags,
    };
}
