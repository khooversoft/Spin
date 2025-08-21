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

public interface IStorePathDetail
{
    string Path { get; }
    bool IsFolder { get; }
    long ContentLength { get; }
    DateTimeOffset? CreatedOn { get; }
    DateTimeOffset LastModified { get; }
    string ETag { get; }
    LeaseStatus LeaseStatus { get; }
    LeaseDuration LeaseDuration { get; }
    string? ContentHash { get; }
}


public record StorePathDetail : IStorePathDetail
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
        ETag = dataETag.ToHash(),
    };

    public static StorePathDetail WithContextHash(this IStorePathDetail subject, string contentHash) => new StorePathDetail
    {
        Path = subject.Path,
        IsFolder = subject.IsFolder,
        ContentLength = subject.ContentLength,
        CreatedOn = subject.CreatedOn,
        LastModified = subject.LastModified,
        ETag = subject.ETag,
        LeaseStatus = subject.LeaseStatus,
        LeaseDuration = subject.LeaseDuration,

        ContentHash = contentHash,
    };
}
