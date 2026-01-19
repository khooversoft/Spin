using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Extensions;

public static class IMemoryCacheExtensions
{
    public static readonly MemoryCacheEntryOptions MemoryOption = new MemoryCacheEntryOptions { SlidingExpiration = TimeSpan.FromMinutes(30) };

    public static void Remove(this IMemoryCache memoryCache, string path) => memoryCache.NotNull().Remove(path);

    public static void Set(this IMemoryCache memoryCache, string path, DataETag data, ILogger logger)
    {
        memoryCache.NotNull();
        path.NotEmpty();

        data.Assert(x => x.Data.Length > 0, "Data length must be greater than zero");
        memoryCache.Set(path, data, MemoryOption);
        logger.LogDebug("Set data to cache, path={path}", path);
    }

    public static void Set<T>(this IMemoryCache memoryCache, string path, T subject, ILogger logger)
    {
        memoryCache.NotNull();
        path.NotEmpty();
        subject.NotNull();

        memoryCache.Set(path, subject, MemoryOption);
        logger.LogDebug("Set type={type} data to cache, path={path}", typeof(T).Name, path);
    }

    public static Option<T> Get<T>(this IMemoryCache memoryCache, string path, ILogger logger)
    {
        if (memoryCache.TryGetValue(path, out T? value))
        {
            logger.LogDebug("Get type={type} data from cache, path={path}", typeof(T).Name, path);
            return value.ToOption();
        }

        logger.LogDebug("Failed to get type={type} data from cache, path={path}", typeof(T).Name, path);
        return StatusCode.NotFound;
    }

    public static bool TryGetValue(this IMemoryCache memoryCache, string path, out DataETag data, ILogger logger)
    {
        if (memoryCache.TryGetValue(path, out DataETag dataETag))
        {
            dataETag.Assert(x => x.Data.Length > 0, "Data length must be greater than zero");
            data = dataETag;

            logger.LogDebug("Get data from cache, path={path}", path);
            return true;
        }

        logger.LogDebug("Failed to get data from cache, path={path}", path);
        data = default;
        return false;
    }

    public static bool TryGetValue<T>(this IMemoryCache memoryCache, string path, out T? subject, ILogger logger)
    {
        if (memoryCache.TryGetValue(path, out T? value))
        {
            subject = value;
            logger.LogDebug("Get type={type} data from cache, path={path}", typeof(T).Name, path);
            return true;
        }

        logger.LogDebug("Failed to get type={type} data from cache, path={path}", typeof(T).Name, path);
        subject = default;
        return false;
    }
}
