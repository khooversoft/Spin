using Azure;
using Toolbox.Tools;

namespace Toolbox.Types;

public struct DataETag
{
    public DataETag(byte[] data) => Data = data.NotNull();
    public DataETag(byte[] data, ETag etag) => (Data, ETag) = (data, etag);

    public byte[] Data { get; }
    public ETag? ETag { get; }

    public static implicit operator DataETag(byte[] data) => new(data);
}
