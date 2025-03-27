using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;

namespace Toolbox.Store;

public class NullMemoryCache : IMemoryCache
{
    public ICacheEntry CreateEntry(object key) => new NullCacheEntry();

    public void Remove(object key) { }

    public bool TryGetValue(object key, out object? value)
    {
        value = default;
        return false;
    }

    public void Dispose() { }
}
