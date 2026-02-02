using System.Collections.Immutable;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.Telemetry;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Store;

public class KeySpace : IKeyStore
{
    private readonly IKeyStore _keyStore;
    private readonly ILogger<KeySpace> _logger;
    private readonly IKeySystem _keySystem;
    private readonly IMemoryCache? _memoryCache;
    private readonly ITelemetryCounter<long>? _addCounter;
    private readonly ITelemetryCounter<long>? _appendCounter;
    private readonly ITelemetryCounter<long>? _deleteCounter;
    private readonly ITelemetryCounter<long>? _getCounter;
    private readonly ITelemetryCounter<long>? _setCounter;
    private readonly ITelemetryCounter<long>? _cacheHitCounter;

    public KeySpace(IKeyStore keyStore, IKeySystem keySystem, ILogger<KeySpace> logger, IMemoryCache? memoryCache = null, ITelemetry? telemetry = null)
    {
        _keyStore = keyStore.NotNull();
        _keySystem = keySystem.NotNull();
        _logger = logger.NotNull();

        _memoryCache = memoryCache;
        _addCounter = telemetry?.CreateCounter<long>("keyspace.add", "Number of Add operations", unit: "count");
        _appendCounter = telemetry?.CreateCounter<long>("keyspace.append", "Number of Append operations", unit: "count");
        _deleteCounter = telemetry?.CreateCounter<long>("keyspace.delete", "Number of Delete operations", unit: "count");
        _getCounter = telemetry?.CreateCounter<long>("keyspace.get", "Number of Get operations", unit: "count");
        _setCounter = telemetry?.CreateCounter<long>("keyspace.set", "Number of Set operations", unit: "count");
        _cacheHitCounter = telemetry?.CreateCounter<long>("keyspace.cache.hit", "Number of Cache Hit operations", unit: "count");
    }

    public IKeySystem KeySystem => _keySystem;

    public async Task<Option<string>> Add(string key, DataETag data)
    {
        var path = _keySystem.PathBuilder(key);
        _logger.LogDebug("Add path={path}", path);

        var result = await _keyStore.Add(path, data);
        if (result.IsOk())
        {
            _memoryCache?.Set(path, data);
            _addCounter?.Increment();
        }

        return result;
    }

    public async Task<Option<string>> Append(string key, DataETag data, string? leaseId = null)
    {
        var path = _keySystem.PathBuilder(key);
        _logger.LogDebug("Appending path={path}", path);

        _memoryCache?.Remove(path);

        var result = await _keyStore.Append(path, data, leaseId: leaseId);
        if (result.IsOk()) _appendCounter?.Increment();

        return result;
    }

    public async Task<Option> Delete(string key, string? leaseId = null)
    {
        var path = _keySystem.PathBuilder(key);
        _logger.LogDebug("Delete path={path}", path);

        _memoryCache?.Remove(path);

        var result = await _keyStore.Delete(path, leaseId: leaseId);
        if (result.IsOk()) _deleteCounter?.Increment();

        return result;
    }

    public Task<Option> DeleteFolder(string key)
    {
        var path = _keySystem.BuildDeleteFolder(key);
        _logger.LogDebug("Delete folder path={path}", path);

        return _keyStore.DeleteFolder(path);
    }

    public async Task<Option<DataETag>> Get(string key)
    {
        var path = _keySystem.PathBuilder(key);
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
        var path = _keySystem.PathBuilder(key);
        _logger.LogDebug("Set path={path}", path);

        var result = await _keyStore.Set(path, data, leaseId: leaseId);
        if (result.IsOk())
        {
            _memoryCache?.Set(path, data);
            _setCounter?.Increment();
        }

        return result;
    }

    public Task<Option> Exists(string key)
    {
        var path = _keySystem.PathBuilder(key);
        _logger.LogDebug("Exists path={path}", path);

        return _keyStore.Exists(path);
    }

    public async Task<Option<StorePathDetail>> GetDetails(string key)
    {
        var path = _keySystem.PathBuilder(key);
        _logger.LogDebug("GetDetails path={path}", path);

        var resultOption = await _keyStore.GetDetails(path);
        if (resultOption.IsError()) return resultOption;

        var result = resultOption.Return();
        return result with { Path = _keySystem.RemovePathPrefix(result.Path) };
    }

    public async Task<IReadOnlyList<StorePathDetail>> Search(string pattern, int index = 0, int size = -1)
    {
        string fullPattern = _keySystem.BuildSearch(pattern);
        _logger.LogDebug("Search fullPattern={fullPattern}", fullPattern);

        var searchResult = await _keyStore.Search(fullPattern, index, size);

        IReadOnlyList<StorePathDetail> result = searchResult
            .Select(x => x with { Path = _keySystem.RemovePathPrefix(x.Path) })
            .ToImmutableArray();

        return result;
    }

    public Task<Option<string>> AcquireExclusiveLock(string key, bool breakLeaseIfExist)
    {
        var path = _keySystem.PathBuilder(key);
        _logger.LogDebug("AcquireExclusiveLock path={path}", path);

        return _keyStore.AcquireExclusiveLock(path, breakLeaseIfExist);
    }

    public Task<Option<string>> AcquireLease(string key, TimeSpan leaseDuration)
    {
        var path = _keySystem.PathBuilder(key);
        _logger.LogDebug("AcquireLease path={path}", path);

        return _keyStore.AcquireLease(path, leaseDuration);
    }

    public Task<Option> BreakLease(string key)
    {
        var path = _keySystem.PathBuilder(key);
        _logger.LogDebug("BreakLease path={path}", path);

        return _keyStore.BreakLease(path);
    }

    public Task<Option> ReleaseLease(string key, string leaseId)
    {
        var path = _keySystem.PathBuilder(key);
        _logger.LogDebug("Release path={path}, leaseId={leaseId}", path, leaseId);
        return _keyStore.ReleaseLease(path, leaseId);
    }

    public Task<Option> RenewLease(string key, string leaseId)
    {
        var path = _keySystem.PathBuilder(key);
        _logger.LogDebug("RenewLease path={path}, leaseId={leaseId}", path, leaseId);
        return _keyStore.RenewLease(path, leaseId);
    }
}
