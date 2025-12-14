using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Toolbox.Data;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Store;

public class KeySpace : IKeyStore
{
    private readonly IKeyStore _keyStore;
    private readonly ILogger<KeySpace> _logger;
    private readonly IKeySystem _keySystem;

    public KeySpace(IKeyStore keyStore, IKeySystem keySystem, ILogger<KeySpace> logger)
    {
        _keyStore = keyStore;
        _keySystem = keySystem;
        _logger = logger;
    }

    public Task<Option<string>> Add(string key, DataETag data, ScopeContext context, TrxRecorder? recorder = null)
    {
        var path = _keySystem.PathBuilder(key);
        context = context.With(_logger);
        context.LogDebug("Add path={path}", path);

        return _keyStore.Add(path, data, context, recorder);
    }

    public Task<Option<string>> Append(string key, DataETag data, ScopeContext context, string? leaseId = null)
    {
        context = context.With(_logger);
        var path = _keySystem.PathBuilder(key);
        context.LogDebug("Appending path={path}", path);

        return _keyStore.Append(path, data, context, leaseId: leaseId);
    }

    public Task<Option> Delete(string key, ScopeContext context, TrxRecorder? recorder = null, string? leaseId = null)
    {
        context = context.With(_logger);
        var path = _keySystem.PathBuilder(key);
        context.LogDebug("Delete path={path}", path);

        return _keyStore.Delete(path, context, recorder: recorder, leaseId: leaseId);
    }

    public Task<Option> DeleteFolder(string key, ScopeContext context)
    {
        context = context.With(_logger);
        var path = _keySystem.BuildDeleteFolder(key);
        context.LogDebug("Delete folder path={path}", path);

        return _keyStore.DeleteFolder(path, context);
    }

    public Task<Option<DataETag>> Get(string key, ScopeContext context)
    {
        context = context.With(_logger);
        var path = _keySystem.PathBuilder(key);
        context.LogDebug("Get path={path}", path);

        return _keyStore.Get(path, context);
    }

    public Task<Option<string>> Set(string key, DataETag data, ScopeContext context, TrxRecorder? recorder = null, string? leaseId = null)
    {
        context = context.With(_logger);
        var path = _keySystem.PathBuilder(key);
        context.LogDebug("Set path={path}", path);

        return _keyStore.Set(path, data, context, recorder: recorder, leaseId: leaseId);
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
        string fullPattern = _keySystem.PathBuilder(pattern);
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

        return _keyStore.AcquireExclusiveLock(path, true, context);
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
