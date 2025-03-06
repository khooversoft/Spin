using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;

namespace Toolbox.Store;

public class NoMemoryCache : IMemoryCache
{
    public ICacheEntry CreateEntry(object key) => new NullCacheEntry();
    public void Remove(object key) { }

    public bool TryGetValue(object key, out object? value)
    {
        value = default;
        return false;
    }

    public void Dispose() { }


    private class NullCacheEntry : ICacheEntry
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
}