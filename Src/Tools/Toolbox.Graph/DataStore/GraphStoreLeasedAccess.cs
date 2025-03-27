using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

public class GraphStoreLeasedAccess : IFileLeasedAccess
{
    private readonly IFileLeasedAccess _fileLeasedAccess;
    private readonly IMemoryCache _memoryCache;
    private bool _disposed = false;
    private static readonly MemoryCacheEntryOptions _memoryOption = new MemoryCacheEntryOptions { SlidingExpiration = TimeSpan.FromMinutes(30) };
    private readonly ILogger _logger;

    public GraphStoreLeasedAccess(IFileLeasedAccess fileLeasedAccess, string path, string leaseId, IMemoryCache memoryCache, ILogger logger)
    {
        _fileLeasedAccess = fileLeasedAccess.NotNull();
        Path = path.NotEmpty();
        LeaseId = leaseId.NotEmpty();
        _memoryCache = memoryCache.NotNull();
        _logger = logger.NotNull();
    }

    public string Path { get; }
    public string LeaseId { get; }
    public DateTime DateAcquired { get; } = DateTime.UtcNow;

    public Task<Option> Append(DataETag data, ScopeContext context)
    {
        _memoryCache.Remove(Path);
        return _fileLeasedAccess.Append(data, context);
    }

    public async Task<Option<DataETag>> Get(ScopeContext context)
    {
        if (_memoryCache.TryGetValue(Path, out DataETag dataETag))
        {
            return dataETag;
        }

        var result = await _fileLeasedAccess.Get(context);
        if (result.IsError()) return result;

        _memoryCache.Set(Path, dataETag, _memoryOption);
        return result;
    }

    public Task<Option> Release(ScopeContext context) => _fileLeasedAccess.Release(context);
    public Task<Option> Renew(ScopeContext context) => _fileLeasedAccess.Renew(context);

    public async Task<Option<string>> Set(DataETag data, ScopeContext context)
    {
        var result = await _fileLeasedAccess.Set(data, context);
        if (result.IsError()) return result;

        _memoryCache.Set(Path, data, _memoryOption);
        return result;
    }

    public async ValueTask DisposeAsync()
    {
        bool disposed = Interlocked.Exchange(ref _disposed, true);
        if (!disposed) await Release(new ScopeContext(_logger)).ConfigureAwait(false);
    }
}
