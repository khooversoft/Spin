using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Types;

namespace Toolbox.Store;

public interface IStorePathDetail
{
    string Path { get; }
    bool IsFolder { get; }
    long ContentLength { get; }
    DateTimeOffset? CreatedOn { get; }
    DateTimeOffset LastModified { get; }
    string ETag { get; }
}


public record StorePathDetail : IStorePathDetail
{
    public string Path { get; init; } = null!;
    public bool IsFolder { get; init; }
    public long ContentLength { get; init; }
    public DateTimeOffset? CreatedOn { get; init; } = DateTimeOffset.UtcNow;
    public DateTimeOffset LastModified { get; init; } = DateTimeOffset.UtcNow;
    public string ETag { get; init; } = null!;
}

public static class StorePathDetailExtensions
{
    public static StorePathDetail ConvertTo(this DataETag dataETag, string path) => new StorePathDetail
    {
        Path = path,
        ContentLength = dataETag.Data.Length,
        ETag = dataETag.ToHash(),
    };
}
