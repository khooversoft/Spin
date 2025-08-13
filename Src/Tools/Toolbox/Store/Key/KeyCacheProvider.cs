using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Store;

public class KeyCacheProvider<T> : IKeyStore<T>
{
    private readonly IFileSystem<T> _fileSystem;
    private readonly ILogger<KeyCacheProvider<T>> _logger;
    private readonly IMemoryCache _memoryCache;
    private readonly TimeSpan _cacheSpan;

    public KeyCacheProvider(IMemoryCache memoryCache, IFileSystem<T> fileSystem, TimeSpan cacheSpan, ILogger<KeyCacheProvider<T>> logger)
    {
        _memoryCache = memoryCache.NotNull();
        _fileSystem = fileSystem.NotNull();
        _cacheSpan = cacheSpan;
        _logger = logger.NotNull();
    }

    public IKeyStore<T>? InnerHandler { get; set; }

    public async Task<Option> Append(string key, T value, ScopeContext context)
    {
        OnDelete(key, context);

        if (InnerHandler != null) return await InnerHandler.Append(key, value, context);
        return StatusCode.OK;
    }

    public async Task<Option> Delete(string key, ScopeContext context)
    {
        OnDelete(key, context);

        if (InnerHandler != null) return await InnerHandler.Delete(key, context);
        return StatusCode.OK;
    }

    public async Task<Option<T>> Get(string key, ScopeContext context)
    {
        string path = _fileSystem.PathBuilder(key);
        context.LogDebug("Getting key={key} from in-memory cache", path);

        if (_memoryCache.TryGetValue(path, out T? value))
        {
            context.LogDebug("CacheHit: Found key={key} in in-memory cache", path);
            return value.NotNull();
        }

        context.LogDebug("CacheMiss: Not found key={key} in in-memory cache", path);
        if (InnerHandler != null)
        {
            var innerOption = await InnerHandler.Get(key, context);
            if (innerOption.IsOk()) OnSet(key, innerOption.Return(), context);

            return innerOption;
        }

        return StatusCode.NotFound;
    }

    public async Task<Option<string>> Set(string key, T value, ScopeContext context)
    {
        OnSet(key, value, context);

        if (InnerHandler != null) return await InnerHandler.Set(key, value, context);
        return value.ToDataETag().ToHash();
    }

    public async Task<Option> AcquireExclusiveLock(string key, ScopeContext context)
    {
        if (InnerHandler != null) return await InnerHandler.AcquireExclusiveLock(key, context);
        return StatusCode.Conflict;
    }

    public async Task<Option> AcquireLock(string key, ScopeContext context)
    {
        if (InnerHandler != null) return await InnerHandler.AcquireLock(key, context);
        return StatusCode.Conflict;
    }
    public async Task<Option> ReleaseLock(string key, ScopeContext context)
    {
        if (InnerHandler != null) return await InnerHandler.ReleaseLock(key, context);
        return StatusCode.Conflict;
    }

    private void OnDelete(string key, ScopeContext context)
    {
        context.LogDebug("CacheDelete: Deleting key={key} from in-memory cache", key);

        string path = _fileSystem.PathBuilder(key);
        _memoryCache.Remove(path);
    }

    private void OnSet(string key, T value, ScopeContext context)
    {
        string path = _fileSystem.PathBuilder(key);
        context.LogDebug("CacheSet: Setting key={key} from in-memory cache", path);

        var cacheOption = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = _cacheSpan,
        };

        _memoryCache.Set(path, value, cacheOption);
    }
}
