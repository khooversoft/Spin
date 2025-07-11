using Microsoft.Extensions.Logging;
using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

public class GraphStoreFileAccess : IFileAccess
{
    private readonly IFileAccess _fileAccess;
    private readonly MemoryCacheAccess _cacheAccess;
    private readonly ILogger _logger;

    public GraphStoreFileAccess(IFileAccess fileAccess, MemoryCacheAccess cacheAccess, ILogger logger)
    {
        _fileAccess = fileAccess.NotNull();
        _cacheAccess = cacheAccess.NotNull();
        _logger = logger.NotNull();
    }

    public string Path => _fileAccess.Path;

    public async Task<Option<IFileLeasedAccess>> Acquire(TimeSpan leaseDuration, ScopeContext context)
    {
        var result = await _fileAccess.Acquire(leaseDuration, context);
        if (result.IsError()) return result;

        return new GraphStoreLeasedAccess(result.Value, Path, result.Value.LeaseId, _cacheAccess.MemoryCache, _logger);
    }

    public Task<Option<IFileLeasedAccess>> AcquireExclusive(bool breakLeaseIfExist, ScopeContext context) => _fileAccess.AcquireExclusive(breakLeaseIfExist, context);

    public async Task<Option<string>> Add(DataETag data, ScopeContext context)
    {
        var result = await _fileAccess.Add(data, context);
        if (result.IsError()) return result;

        _cacheAccess.Set(Path, data);
        return result;
    }

    public Task<Option<string>> Append(DataETag data, ScopeContext context)
    {
        _cacheAccess.Remove(Path);
        return _fileAccess.Append(data, context);
    }

    public Task<Option> BreakLease(ScopeContext context) => _fileAccess.BreakLease(context);

    public Task<Option> Delete(ScopeContext context)
    {
        _cacheAccess.Remove(Path);
        return _fileAccess.Delete(context);
    }

    public Task<Option> Exists(ScopeContext context) => _fileAccess.Exists(context);

    public async Task<Option<DataETag>> Get(ScopeContext context)
    {
        if (_cacheAccess.TryGetValue(Path, out DataETag dataETag))
        {
            return dataETag;
        }

        Option<DataETag> result = await _fileAccess.Get(context);
        if (result.IsError()) return result;

        DataETag data = result.Return();
        _cacheAccess.Set(Path, data);
        return result;
    }

    public Task<Option<IStorePathDetail>> GetDetails(ScopeContext context) => _fileAccess.GetDetails(context);

    public async Task<Option<string>> Set(DataETag dataETag, ScopeContext context)
    {
        var result = await _fileAccess.Set(dataETag, context);
        if (result.IsError()) return result;

        _cacheAccess.Set(Path, dataETag);
        return result;
    }
}
