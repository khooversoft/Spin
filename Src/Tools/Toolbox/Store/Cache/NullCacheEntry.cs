using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;

namespace Toolbox.Store;

public class NullCacheEntry : ICacheEntry
{
    public object Key { get; set; } = default!;
    public object? Value { get; set; }
    public DateTimeOffset? AbsoluteExpiration { get; set; }
    public TimeSpan? AbsoluteExpirationRelativeToNow { get; set; }
    public TimeSpan? SlidingExpiration { get; set; }
    public IList<IChangeToken> ExpirationTokens { get; } = new List<IChangeToken>();
    public IList<PostEvictionCallbackRegistration> PostEvictionCallbacks { get; } = new List<PostEvictionCallbackRegistration>();
    public CacheItemPriority Priority { get; set; }
    public long? Size { get; set; }
    public void Dispose() { }
}
