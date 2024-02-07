using System.Collections.Concurrent;
using System.Threading.Tasks.Dataflow;
using Microsoft.Extensions.Logging;
using Toolbox.Azure.DataLake;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace NBlog.sdk;

public class StateManagement
{
    private readonly ILogger<StateManagement> _logger;
    private readonly ConcurrentDictionary<string, CacheObject<object>> _cacheDataMap = new(StringComparer.OrdinalIgnoreCase);
    private string? _eTag;
    private TimeSpan _cacheValueSpan = TimeSpan.FromMinutes(30);
    private TimeSpan _checkSpan = TimeSpan.FromMinutes(5);
    private DateTime _refreshTime;
    private readonly ActionBlock<ScopeContext> _checkDirectoryFile;
    private readonly IDatalakeStore _datalakeStore;

    public StateManagement(IDatalakeStore datalakeStore, ILogger<StateManagement> logger)
    {
        _datalakeStore = datalakeStore.NotNull();
        _logger = logger.NotNull();

        _checkDirectoryFile = new ActionBlock<ScopeContext>(CheckDirectoryFile, new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 1 });
        _refreshTime = DateTime.Now + _checkSpan;
    }

    public void Clear() => _cacheDataMap.Clear();
    public void Clear(string name) => _cacheDataMap.TryRemove(name, out _);

    public Option<T> Get<T>(string name, ScopeContext context)
    {
        context = context.With(_logger);

        _checkDirectoryFile.Post(context);

        if (_cacheDataMap.TryGetValue(name, out var cacheObject) && cacheObject.TryGetValue(out var value))
        {
            context.LogInformation("Cache hit for name={name}", name);
            return value.Cast<T>();
        }

        return StatusCode.NotFound;
    }

    public void Set<T>(string name, T value, ScopeContext context)
    {
        context = context.With(_logger);

        context.LogInformation("Setting cache for name={name}", name);
        _cacheDataMap[name] = new CacheObject<object>(_cacheValueSpan).Set(value);
    }

    private async Task CheckDirectoryFile(ScopeContext context)
    {
        if (DateTime.Now < _refreshTime) return;

        context.LogInformation("Refresh time has expired, refreshing");
        _refreshTime = DateTime.Now + _checkSpan;

        context.LogInformation("Checking ETag of directory file={file}", NBlogConstants.DirectoryActorKey);

        Option<DatalakePathProperties> readPathPropertyOption = await GetPathProperties(NBlogConstants.DirectoryActorKey, context);
        if (readPathPropertyOption.IsError()) return;

        DatalakePathProperties pathProperty = readPathPropertyOption.Return();

        string e1 = pathProperty.ETag.ToString();
        if (e1 == _eTag)
        {
            context.LogInformation("Directory's file={file} ETag has not changed, ETag={etag}, {store.etag}", NBlogConstants.DirectoryActorKey, e1, _eTag);
            return;
        }

        context.LogInformation("Directory's file={file} ETag has changed, ETag={etag}, {store.etag}", NBlogConstants.DirectoryActorKey, e1, _eTag);
        _eTag = e1;
        _cacheDataMap.Clear();
        return;
    }

    private async Task<Option<DatalakePathProperties>> GetPathProperties(string fileId, ScopeContext context)
    {
        context = context.With(_logger);
        context.LogInformation("Getting properties of file={file}", fileId);

        Option<DatalakePathProperties> propertyOption = await _datalakeStore.GetPathProperties(fileId, context);
        if (propertyOption.IsError())
        {
            context.LogError("Failed to read path properties for file={file}", fileId);
            return propertyOption;
        }

        return propertyOption;
    }
}
