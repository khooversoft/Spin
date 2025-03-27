using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

public class GraphStoreFileAccess : IFileAccess
{
    private readonly IFileAccess _fileAccess;
    private readonly IMemoryCache _memoryCache;
    private static readonly MemoryCacheEntryOptions _memoryOption = new MemoryCacheEntryOptions { SlidingExpiration = TimeSpan.FromMinutes(30) };
    private readonly ILogger _logger;

    public GraphStoreFileAccess(IFileAccess fileAccess, IMemoryCache memoryCache, ILogger logger)
    {
        _fileAccess = fileAccess.NotNull();
        _memoryCache = memoryCache.NotNull();
        _logger = logger.NotNull();
    }

    public string Path => _fileAccess.Path;

    public async Task<Option<IFileLeasedAccess>> Acquire(TimeSpan leaseDuration, ScopeContext context)
    {
        var result = await _fileAccess.Acquire(leaseDuration, context);
        if (result.IsError()) return result;

        return new GraphStoreLeasedAccess(result.Value, Path, result.Value.LeaseId, _memoryCache, _logger);
    }

    public Task<Option<IFileLeasedAccess>> AcquireExclusive(ScopeContext context) => _fileAccess.AcquireExclusive(context);

    public async Task<Option<string>> Add(DataETag data, ScopeContext context)
    {
        var result = await _fileAccess.Add(data, context);
        if (result.IsError()) return result;

        _memoryCache.Set(Path, data, _memoryOption);
        return result;
    }

    public Task<Option> Append(DataETag data, ScopeContext context)
    {
        _memoryCache.Remove(Path);
        return _fileAccess.Append(data, context);
    }

    public Task<Option> BreakLease(ScopeContext context) => _fileAccess.BreakLease(context);

    public Task<Option> ClearLease(ScopeContext context) => _fileAccess.ClearLease(context);

    public Task<Option> Delete(ScopeContext context)
    {
        _memoryCache.Remove(Path);
        return _fileAccess.Delete(context);
    }

    public Task<Option> Exist(ScopeContext context) => _fileAccess.Exist(context);

    public async Task<Option<DataETag>> Get(ScopeContext context)
    {
        if (_memoryCache.TryGetValue(Path, out DataETag dataETag))
        {
            return dataETag;
        }

        var result = await _fileAccess.Get(context);
        if (result.IsError()) return result;

        _memoryCache.Set(Path, dataETag, _memoryOption);
        return result;
    }

    public Task<Option<IStorePathDetail>> GetDetail(ScopeContext context) => _fileAccess.GetDetail(context);

    public async Task<Option<string>> Set(DataETag data, ScopeContext context)
    {
        var result = await _fileAccess.Set(data, context);
        if (result.IsError()) return result;

        _memoryCache.Set(Path, data, _memoryOption);
        return result;
    }
}
