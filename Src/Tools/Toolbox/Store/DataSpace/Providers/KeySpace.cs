using System.Collections.Immutable;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Toolbox.Data;
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

    public async Task<Option<string>> Add(string key, DataETag data, ScopeContext context, TrxRecorder? recorder = null)
    {
        var path = _keySystem.PathBuilder(key);
        context = context.With(_logger);
        context.LogDebug("Add path={path}", path);

        var result = await _keyStore.Add(path, data, context, recorder);
        if (result.IsOk()) _memoryCache?.Set(path, data, context);

        _addCounter?.Increment();
        return result;
    }

    public async Task<Option<string>> Append(string key, DataETag data, ScopeContext context, string? leaseId = null)
    {
        context = context.With(_logger);
        var path = _keySystem.PathBuilder(key);
        context.LogDebug("Appending path={path}", path);

        var result = await _keyStore.Append(path, data, context, leaseId: leaseId);
        if (result.IsOk()) _memoryCache?.Remove(path);

        _appendCounter?.Increment();
        return result;
    }

    public async Task<Option> Delete(string key, ScopeContext context, TrxRecorder? recorder = null, string? leaseId = null)
    {
        context = context.With(_logger);
        var path = _keySystem.PathBuilder(key);
        context.LogDebug("Delete path={path}", path);

        var result = await _keyStore.Delete(path, context, recorder: recorder, leaseId: leaseId);
        if (result.IsOk()) _memoryCache?.Remove(path);

        _deleteCounter?.Increment();
        return result;
    }

    public Task<Option> DeleteFolder(string key, ScopeContext context)
    {
        context = context.With(_logger);
        var path = _keySystem.BuildDeleteFolder(key);
        context.LogDebug("Delete folder path={path}", path);

        return _keyStore.DeleteFolder(path, context);
    }

    public async Task<Option<DataETag>> Get(string key, ScopeContext context)
    {
        context = context.With(_logger);
        var path = _keySystem.PathBuilder(key);
        context.LogDebug("Get path={path}", path);

        if (_memoryCache?.TryGetValue(path, out DataETag cachedData, context) == true)
        {
            _cacheHitCounter?.Increment();
            return cachedData;
        }

        var result = await _keyStore.Get(path, context);
        if (result.IsOk()) _memoryCache?.Set(path, result.Return(), context);

        _getCounter?.Increment();
        return result;
    }

    public async Task<Option<string>> Set(string key, DataETag data, ScopeContext context, TrxRecorder? recorder = null, string? leaseId = null)
    {
        context = context.With(_logger);
        var path = _keySystem.PathBuilder(key);
        context.LogDebug("Set path={path}", path);

        var result = await _keyStore.Set(path, data, context, recorder: recorder, leaseId: leaseId);
        if (result.IsOk()) _memoryCache?.Set(path, data, context);

        _setCounter?.Increment();
        return result;
    }

    public Task<Option> Exists(string key, ScopeContext context)
    {
        context = context.With(_logger);
        var path = _keySystem.PathBuilder(key);
        context.LogDebug("Exists path={path}", path);

        return _keyStore.Exists(path, context);
    }

    public async Task<Option<StorePathDetail>> GetDetails(string key, ScopeContext context)
    {
        context = context.With(_logger);
        var path = _keySystem.PathBuilder(key);
        context.LogDebug("GetDetails path={path}", path);

        var resultOption = await _keyStore.GetDetails(path, context);
        if (resultOption.IsError()) return resultOption;

        var result = resultOption.Return();
        return result with { Path = _keySystem.RemovePathPrefix(result.Path) };
    }

    public async Task<IReadOnlyList<StorePathDetail>> Search(string pattern, ScopeContext context)
    {
        context = context.With(_logger);
        string fullPattern = _keySystem.BuildSearch(null, pattern);
        context.LogDebug("Search fullPattern={fullPattern}", fullPattern);

        var searchResult = await _keyStore.Search(fullPattern, context);

        IReadOnlyList<StorePathDetail> result = searchResult
            .Select(x => x with { Path = _keySystem.RemovePathPrefix(x.Path) })
            .ToImmutableArray();

        return result;
    }

    public Task<Option<string>> AcquireExclusiveLock(string key, bool breakLeaseIfExist, ScopeContext context)
    {
        context = context.With(_logger);
        var path = _keySystem.PathBuilder(key);
        context.LogDebug("AcquireExclusiveLock path={path}", path);

        return _keyStore.AcquireExclusiveLock(path, breakLeaseIfExist, context);
    }

    public Task<Option<string>> AcquireLease(string key, TimeSpan leaseDuration, ScopeContext context)
    {
        context = context.With(_logger);
        var path = _keySystem.PathBuilder(key);
        context.LogDebug("AcquireLease path={path}", path);

        return _keyStore.AcquireLease(path, leaseDuration, context);
    }

    public Task<Option> BreakLease(string key, ScopeContext context)
    {
        context = context.With(_logger);
        var path = _keySystem.PathBuilder(key);
        context.LogDebug("BreakLease path={path}", path);

        return _keyStore.BreakLease(path, context);
    }

    public Task<Option> Release(string leaseId, ScopeContext context)
    {
        context = context.With(_logger);
        context.LogDebug("Release leaseId={leaseId}", leaseId);

        return _keyStore.Release(leaseId, context);
    }
}
