using System.Diagnostics.CodeAnalysis;
using Azure;
using Toolbox.Tools;

namespace Toolbox.Types;

public struct DataETag
{
    public DataETag() { }

    [SetsRequiredMembers]
    public DataETag(byte[] data) => Data = data.NotNull();

    [SetsRequiredMembers]
    public DataETag(byte[] data, ETag etag) => (Data, ETag) = (data, etag);

    public required byte[] Data { get; init; }
    public ETag? ETag { get; init; }

    public static implicit operator DataETag(byte[] data) => new(data);
}
