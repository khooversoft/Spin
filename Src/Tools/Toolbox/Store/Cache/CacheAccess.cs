using Microsoft.Extensions.Caching.Memory;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Store;

public class MemoryCacheAccess
{
    private static readonly MemoryCacheEntryOptions _memoryOption = new MemoryCacheEntryOptions { SlidingExpiration = TimeSpan.FromMinutes(30) };

    public MemoryCacheAccess(IMemoryCache memoryCache) => MemoryCache = memoryCache.NotNull();

    public IMemoryCache MemoryCache { get; }
    public void Remove(string path) => MemoryCache.Remove(path);

    public void Set(string path, DataETag data)
    {
        path.NotEmpty();
        data.Assert(x => x.Data.Length > 0, "Data length must be greater than zero");
        MemoryCache.Set(path, data, _memoryOption);
    }

    public bool TryGetValue(string path, out DataETag data)
    {
        if (MemoryCache.TryGetValue(path, out DataETag dataETag))
        {
            dataETag.Assert(x => x.Data.Length > 0, "Data length must be greater than zero");
            data = dataETag;
            return true;
        }

        data = default;
        return false;
    }
}
