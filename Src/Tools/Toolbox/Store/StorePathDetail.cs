using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Store;

public enum LeaseStatus
{
    Locked,
    Unlocked,
}

public enum LeaseDuration
{
    Infinite,
    Fixed
}


public record StorePathDetail
{
    public string Path { get; init; } = null!;
    public bool IsFolder { get; init; }
    public long ContentLength { get; init; }
    public DateTimeOffset? CreatedOn { get; init; } = DateTimeOffset.UtcNow;
    public DateTimeOffset LastModified { get; init; } = DateTimeOffset.UtcNow;
    public string ETag { get; init; } = null!;
    public LeaseStatus LeaseStatus { get; init; }
    public LeaseDuration LeaseDuration { get; init; }
    public string? ContentHash { get; init; }
}

public static class StorePathDetailExtensions
{
    public static StorePathDetail ConvertTo(this DataETag dataETag, string path) => new StorePathDetail
    {
        Path = path,
        ContentLength = dataETag.Data.Length,
        ETag = dataETag.ETag.NotEmpty(),
    };

    public static StorePathDetail WithContextHash(this StorePathDetail subject, string contentHash) => subject with
    {
        ContentHash = contentHash,
    };
}
