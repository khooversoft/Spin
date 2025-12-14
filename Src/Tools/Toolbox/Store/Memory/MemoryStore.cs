using System.Collections.Concurrent;
using System.Collections.Immutable;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Toolbox.Data;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Store;

public class MemoryStore
{
    private readonly ConcurrentDictionary<string, DirectoryDetail> _store = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, LeaseRecord> _leaseStore = new(StringComparer.OrdinalIgnoreCase);
    private readonly string _storeName = "MemoryStore-" + Guid.NewGuid().ToString();
    private readonly object _lock = new object();
    private readonly ILogger<MemoryStore> _logger;

    public MemoryStore(ILogger<MemoryStore> logger) => _logger = logger.NotNull();

    public MemoryStore(TransactionManagerOption trxManagerOption, IServiceProvider serviceProvider, ILogger<MemoryStore> logger)
    {
        _logger = logger.NotNull();
        serviceProvider.NotNull();
    }

    public string Name => _storeName;

    public Option<string> Add(string path, DataETag data, ScopeContext context, TrxRecorder? recorder = null)
    {
        context = context.With(_logger);
        if (path.IsEmpty()) return (StatusCode.BadRequest, "Path is required");
        path = RemoveForwardSlash(path);

        if (!FileStoreTool.IsPathValid(path)) return (StatusCode.BadRequest, "Path is invalid");

        lock (_lock)
        {
            if (IsLeased(path)) return (StatusCode.Conflict, "Path is leased");

            DirectoryDetail detail = new(data.ConvertTo(path), data, null);

            Option<string> result = _store.TryAdd(path, detail) switch
            {
                true => detail.PathDetail.ETag.NotEmpty().ToOption(),
                false => StatusCode.Conflict,
            };

            if (result.IsOk()) recorder?.Add(_storeName, path, detail);

            context.LogDebug("Add Path={path}, length={length}", path, data.Data.Length);
            return result;
        }
    }

    public Option<string> Append(string path, DataETag data, string? leaseId, ScopeContext context)
    {
        context = context.With(_logger);
        path = RemoveForwardSlash(path);

        lock (_lock)
        {
            if (IsLeased(path, leaseId)) return (StatusCode.Conflict, "Path is leased");

            StorePathDetail pathDetail = data.ConvertTo(path);

            if (_store.TryGetValue(path, out var readPayload))
            {
                var newPayload = readPayload with { Data = (readPayload.Data + data).WithHash() };
                _store[path] = newPayload;
                context.LogDebug("Append Path={path}, length={length}", path, data.Data.Length);
                return newPayload.Data.ETag.NotEmpty();
            }

            DirectoryDetail detail = new(pathDetail, data.WithHash());
            _store[path] = detail;
            return detail.Data.ETag.NotEmpty();
        }
    }

    public Option DeleteFolder(string pattern, ScopeContext context)
    {
        var query = QueryParameter.Parse(pattern);
        var matcher = query.GetMatcher();

        var result = _store.Keys
            .Where(x => matcher.IsMatch(x, false))
            .Select(x => Delete(x, null, context))
            .ToList();

        return StatusCode.OK;
    }

    public bool Exist(string path) => _store.ContainsKey(RemoveForwardSlash(path));

    public Option<DataETag> Get(string path) => _store.TryGetValue(RemoveForwardSlash(path), out var payload) switch
    {
        true => payload.Data,
        false => StatusCode.NotFound,
    };

    public Option Delete(string path, string? leaseId, ScopeContext context, TrxRecorder? recorder = null)
    {
        context = context.With(_logger);
        path = RemoveForwardSlash(path);

        lock (_lock)
        {
            if (IsLeased(path, leaseId)) return (StatusCode.Locked, "Path is leased");

            Option result = _store.TryRemove(path, out var payload) switch
            {
                true when payload.LeaseRecord != null => _leaseStore.TryRemove(payload.LeaseRecord.LeaseId, out _) ? StatusCode.OK : StatusCode.Conflict,
                true => StatusCode.OK,
                _ => StatusCode.NotFound,
            };

            if (result.IsOk()) recorder?.Delete(_storeName, path, payload!);

            result.LogStatus(context, "Remove Path={path}, leaseId={leaseId}", [path, leaseId ?? "<no leaseId>"]);
            return result;
        }
    }

    public Option<StorePathDetail> GetDetail(string path)
    {
        path = RemoveForwardSlash(path);

        return _store.TryGetValue(path, out var payload) switch
        {
            true => payload.LeaseRecord switch
            {
                null => payload.PathDetail with { LeaseStatus = LeaseStatus.Unlocked, LeaseDuration = LeaseDuration.Infinite },
                var v => payload.PathDetail with
                {
                    LeaseDuration = v.Infinite ? LeaseDuration.Infinite : LeaseDuration.Fixed,
                    LeaseStatus = LeaseStatus.Locked,
                },
            },

            false => StatusCode.NotFound,
        };
    }

    public Option<string> Set(string path, DataETag data, string? leaseId, ScopeContext context, TrxRecorder? recorder = null)
    {
        context = context.With(_logger);
        path = RemoveForwardSlash(path);

        if (!FileStoreTool.IsPathValid(path)) return (StatusCode.BadRequest, "Path is invalid");

        lock (_lock)
        {
            if (IsLeased(path, leaseId)) return (StatusCode.Locked, "Path is leased");

            var result = _store.AddOrUpdate(path,
                x =>
                {
                    var p = new DirectoryDetail(data.ConvertTo(x), data, null);
                    recorder?.Add(_storeName, x, p);
                    return p;
                },
                (x, current) =>
                {
                    var payload = current with
                    {
                        Data = data,
                        PathDetail = current.PathDetail with
                        {
                            LastModified = DateTimeOffset.UtcNow,
                            ETag = data.ToHash(),
                        },
                    };

                    recorder?.Update(_storeName, x, current, payload);
                    return payload;
                });

            context.LogDebug("Set Path={path}, length={length}", path, data.Data.Length);
            return result.PathDetail.ETag;
        }
    }

    public IReadOnlyList<StorePathDetail> Search(string pattern)
    {
        pattern = RemoveForwardSlash(pattern);
        var query = QueryParameter.Parse(pattern).GetMatcher();

        var list = _store.Values
            .Where(x => pattern == "*" || query.IsMatch(x.PathDetail.Path, false))
            .Select(x => x.PathDetail with { Path = RemoveForwardSlash(x.PathDetail.Path) })
            .ToImmutableArray();

        return list;
    }

    public IReadOnlyList<(StorePathDetail Detail, DataETag Data)> SearchData(string pattern)
    {
        var query = QueryParameter.Parse(pattern).GetMatcher();

        var list = _store.Values
            .Select(x => (Detail: x.PathDetail, Data: x.Data))
            .Where(x => pattern == "*" || query.IsMatch(x.Detail.Path, false))
            .ToImmutableArray();

        return list;
    }

    // ========================================================================
    // Lease

    public Option<LeaseRecord> AcquireLease(string path, TimeSpan leaseDuration, ScopeContext context)
    {
        context = context.With(_logger);
        path = RemoveForwardSlash(path);

        LeaseRecord? leaseRecord = null!;
        Option result = StatusCode.OK;

        lock (_lock)
        {
            DirectoryDetail directoryDetail = _store.AddOrUpdate(path,
                x =>
                {
                    var data = new byte[0].ToDataETag();
                    StorePathDetail storePathDetail = data.ConvertTo(path);
                    leaseRecord = new LeaseRecord(path, leaseDuration);
                    _leaseStore[leaseRecord.LeaseId] = leaseRecord;

                    DirectoryDetail dirDetail = new DirectoryDetail(storePathDetail, data, leaseRecord);
                    result = StatusCode.OK;
                    return dirDetail;
                },
                (x, v) =>
                {
                    result = v.LeaseRecord switch
                    {
                        LeaseRecord i when i.IsLeaseValid() == true => (StatusCode.Locked, "Path is already leased"),
                        _ => StatusCode.OK,
                    };

                    if (result.IsError())
                    {
                        context.LogError("Failed to acquire lease, path={path}, current leaseId={leaseId}", path, v.LeaseRecord?.LeaseId);
                        return v;
                    }

                    leaseRecord = new LeaseRecord(path, leaseDuration);
                    var dirDetail = v with { LeaseRecord = leaseRecord };
                    _leaseStore[leaseRecord.LeaseId] = leaseRecord;

                    context.LogDebug("Acquire lease Path={path}, leaseId={leaseId}", path, leaseRecord.LeaseId);
                    return dirDetail;
                });

            return new Option<LeaseRecord>(leaseRecord, result.StatusCode, result.Error);
        }
    }

    public Option BreakLease(string path, ScopeContext context)
    {
        path = RemoveForwardSlash(path);
        context = context.With(_logger);

        lock (_lock)
        {
            if (!_store.TryGetValue(path, out var payload)) return (StatusCode.NotFound, "Lease not found");

            if (payload.LeaseRecord != null)
            {
                _leaseStore.Remove(payload.LeaseRecord.LeaseId, out var _);
                _store[path] = payload with { LeaseRecord = null };
            }

            context.LogDebug("Break lease Path={path}", path);
            return StatusCode.OK;
        }
    }

    public Option ReleaseLease(string leaseId, ScopeContext context)
    {
        context = context.With(_logger);

        lock (_lock)
        {
            if (!_leaseStore.TryRemove(leaseId, out var leaseRecord)) return (StatusCode.NotFound, "Lease not found");

            _store[leaseRecord.Path] = _store[leaseRecord.Path] with { LeaseRecord = null };
            context.LogDebug("Release lease Path={path}, leaseId={leaseId}", leaseRecord.Path, leaseId);
            return StatusCode.OK;
        }
    }

    public bool IsLeased(string path, string? leaseId = null)
    {
        path = RemoveForwardSlash(path);

        // Path does not exist or there is no lease
        if (!_store.TryGetValue(path, out var directoryDetail) || directoryDetail.LeaseRecord == null) return false;

        // Lease is no longer valid, clean up
        if (!directoryDetail.LeaseRecord.IsLeaseValid())
        {
            string currentLeaseId = directoryDetail.LeaseRecord.LeaseId;

            _leaseStore.TryRemove(directoryDetail.LeaseRecord.LeaseId, out var _);
            _store[path] = directoryDetail with { LeaseRecord = null };
            return false;
        }

        if (directoryDetail.LeaseRecord.IsLeaseValid(leaseId) == true) return true;
        return false;
    }

    private static string RemoveForwardSlash(string path) => path.NotEmpty().StartsWith('/') switch
    {
        true => path[1..],
        false => path,
    };
}
