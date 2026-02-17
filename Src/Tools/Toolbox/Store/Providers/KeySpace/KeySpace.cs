using System.Collections.Immutable;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.Telemetry;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Store;

public partial class KeySpace : IKeyStore
{
    private readonly IKeyStore _keyStore;
    private readonly ILogger<KeySpace> _logger;
    private readonly IKeyPathStrategy _keyPathStrategy;
    private readonly IMemoryCache? _memoryCache;
    private readonly ITelemetryCounter<long>? _addCounter;
    private readonly ITelemetryCounter<long>? _appendCounter;
    private readonly ITelemetryCounter<long>? _deleteCounter;
    private readonly ITelemetryCounter<long>? _getCounter;
    private readonly ITelemetryCounter<long>? _setCounter;
    private readonly ITelemetryCounter<long>? _cacheHitCounter;

    public KeySpace(IKeyStore keyStore, IKeyPathStrategy keySystem, ILogger<KeySpace> logger, IMemoryCache? memoryCache = null, ITelemetry? telemetry = null)
    {
        _keyStore = keyStore.NotNull();
        _keyPathStrategy = keySystem.NotNull();
        _logger = logger.NotNull();

        _memoryCache = memoryCache;
        _addCounter = telemetry?.CreateCounter<long>("keyspace.add", "Number of Add operations", unit: "count");
        _appendCounter = telemetry?.CreateCounter<long>("keyspace.append", "Number of Append operations", unit: "count");
        _deleteCounter = telemetry?.CreateCounter<long>("keyspace.delete", "Number of Delete operations", unit: "count");
        _getCounter = telemetry?.CreateCounter<long>("keyspace.get", "Number of Get operations", unit: "count");
        _setCounter = telemetry?.CreateCounter<long>("keyspace.set", "Number of Set operations", unit: "count");
        _cacheHitCounter = telemetry?.CreateCounter<long>("keyspace.cache.hit", "Number of Cache Hit operations", unit: "count");
    }

    public IKeyPathStrategy KeyPathStrategy => _keyPathStrategy;

    public async Task<Option<string>> Add(string key, DataETag data)
    {
        var path = _keyPathStrategy.BuildPath(key);
        _logger.LogDebug("Add path={path}", path);

        var result = await _keyStore.Add(path, data);
        if (result.IsOk())
        {
            _memoryCache?.Set(path, data);
            _addCounter?.Increment();
            _recorder?.Add(path, data);
        }

        return result;
    }

    public async Task<Option<string>> Append(string key, DataETag data, string? leaseId = null)
    {
        var path = _keyPathStrategy.BuildPath(key);
        _logger.LogDebug("Appending path={path}", path);

        _memoryCache?.Remove(path);

        var result = await _keyStore.Append(path, data, leaseId: leaseId);
        if (result.IsOk()) _appendCounter?.Increment();

        return result;
    }

    public async Task<Option> Delete(string key, string? leaseId = null)
    {
        var path = _keyPathStrategy.BuildPath(key);
        _logger.LogDebug("Delete path={path}", path);

        _memoryCache?.Remove(path);

        Option<DataETag> readOption = StatusCode.NotFound;
        if (_recorder != null)
        {
            readOption = await Get(key);
            if (readOption.IsError()) return readOption.ToOptionStatus();
        }

        var result = await _keyStore.Delete(path, leaseId: leaseId);
        if (result.IsOk()) _deleteCounter?.Increment();

        _recorder?.Delete(path, readOption.Return());
        return result;
    }

    public async Task<Option> DeleteFolder(string key)
    {
        if (_keyPathStrategy is KeyPathStrategy keyPathStrategy)
        {
            var path = keyPathStrategy.BuildDeleteFolder(key);
            _logger.LogDebug("Delete folder path={path}", path);
            return await _keyStore.DeleteFolder(path);
        }

        var keySearch = _keyPathStrategy.BuildKeySearch(key);
        var fileList = await Search(keySearch);
        foreach (var file in fileList)
        {
            string onlyKey = _keyPathStrategy.ExtractKey(file.Path);
            await Delete(onlyKey);
        }

        return StatusCode.OK;
    }

    public async Task<Option<DataETag>> Get(string key)
    {
        var path = _keyPathStrategy.BuildPath(key);
        _logger.LogDebug("Get path={path}", path);

        if (_memoryCache?.TryGetValue(path, out DataETag? cachedData) == true)
        {
            _cacheHitCounter?.Increment();
            return cachedData.NotNull();
        }

        var result = await _keyStore.Get(path);
        if (result.IsOk())
        {
            _memoryCache?.Set(path, result.Return());
            _getCounter?.Increment();
        }

        return result;
    }

    public async Task<Option<string>> Set(string key, DataETag data, string? leaseId = null)
    {
        var path = _keyPathStrategy.BuildPath(key);
        _logger.LogDebug("Set path={path}", path);

        Option<DataETag> currentDataOption = StatusCode.NotFound;
        if (_recorder != null) currentDataOption = await Get(key);

        var result = await _keyStore.Set(path, data, leaseId: leaseId);
        if (result.IsOk())
        {
            _memoryCache?.Set(path, data);
            _setCounter?.Increment();
        }

        if (_recorder != null)
        {
            if (currentDataOption.IsOk())
                _recorder?.Update(path, currentDataOption.Return(), data);
            else
                _recorder?.Add(path, data);
        }

        return result;
    }

    public Task<Option> Exists(string key)
    {
        var path = _keyPathStrategy.BuildPath(key);
        _logger.LogDebug("Exists path={path}", path);

        return _keyStore.Exists(path);
    }

    public async Task<Option<StorePathDetail>> GetDetails(string key)
    {
        var path = _keyPathStrategy.BuildPath(key);
        _logger.LogDebug("GetDetails path={path}", path);

        var resultOption = await _keyStore.GetDetails(path);
        if (resultOption.IsError()) return resultOption;

        var result = resultOption.Return();
        return result with { Path = _keyPathStrategy.RemoveBasePath(result.Path) };
    }

    public async Task<IReadOnlyList<StorePathDetail>> Search(string pattern, int index = 0, int size = -1)
    {
        string fullPattern = _keyPathStrategy.BuildSearch(pattern);
        _logger.LogDebug("Search fullPattern={fullPattern}", fullPattern);

        var searchResult = await _keyStore.Search(fullPattern, index, size);

        IReadOnlyList<StorePathDetail> result = searchResult
            .Select(x => x with { Path = _keyPathStrategy.RemoveBasePath(x.Path) })
            .ToImmutableArray();

        return result;
    }
}
