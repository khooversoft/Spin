using System.Collections.Concurrent;
using System.Collections.Immutable;
using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.Logging;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Store;

public sealed class MemoryStore
{
    private record DirectoryDetail(StorePathDetail PathDetail, DataETag Data, LeaseRecord? LeaseRecord = null);

    private readonly ConcurrentDictionary<string, DirectoryDetail> _store = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, LeaseRecord> _leaseStore = new(StringComparer.OrdinalIgnoreCase);
    private readonly object _lock = new object();
    private readonly ILogger<MemoryStore> _logger;

    public MemoryStore(ILogger<MemoryStore> logger) => _logger = logger.NotNull();

    public Option<string> Add(string path, DataETag data, ScopeContext context)
    {
        context = context.With(_logger);

        if (!FileStoreTool.IsPathValid(path)) return (StatusCode.BadRequest, "Path is invalid");

        lock (_lock)
        {
            if (IsLeased(path)) return (StatusCode.Conflict, "Path is leased");

            DirectoryDetail detail = new(data.ConvertTo(path), data, null);
            var result = _store.TryAdd(path, detail) switch
            {
                true => StatusCode.OK,
                false => StatusCode.Conflict,
            };

            context.LogTrace("Add Path={path}, length={length}", path, data.Data.Length);
            return detail.Data.ETag.NotEmpty();
        }
    }

    public Option<string> Append(string path, DataETag data, string? leaseId, ScopeContext context)
    {
        context = context.With(_logger);

        lock (_lock)
        {
            if (IsLeased(path, leaseId)) return (StatusCode.Conflict, "Path is leased");

            StorePathDetail pathDetail = data.ConvertTo(path);

            if (_store.TryGetValue(path, out var readPayload))
            {
                var newPayload = readPayload with { Data = readPayload.Data.Data.Concat(data.Data).ToDataETag().WithHash() };
                _store[path] = newPayload;
                context.LogTrace("Append Path={path}, length={length}", path, data.Data.Length);
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

        var restul = _store.Keys
            .Where(x => matcher.IsMatch(x, false))
            .Select(x => Delete(x, null, context))
            .ToList();

        return StatusCode.OK;
    }

    public bool Exist(string path) => _store.ContainsKey(path);

    public Option<DataETag> Get(string path) => _store.TryGetValue(path, out var payload) switch
    {
        true => payload.Data,
        false => StatusCode.NotFound,
    };

    public Option Delete(string path, string? leaseId, ScopeContext context)
    {
        context = context.With(_logger);

        lock (_lock)
        {
            if (IsLeased(path, leaseId)) return (StatusCode.Conflict, "Path is leased");

            Option result = _store.TryRemove(path, out var payload) switch
            {
                true when payload.LeaseRecord != null => _leaseStore.TryRemove(payload.LeaseRecord.LeaseId, out _) ? StatusCode.OK : StatusCode.Conflict,
                true => StatusCode.OK,
                _ => StatusCode.NotFound,
            };

            result.LogStatus(context, "Remove Path={path}, leaseId={leaseId}", [path, leaseId ?? "<no leaseId>"]);
            return result;
        }
    }

    public Option<IStorePathDetail> GetDetail(string path)
    {
        return _store.TryGetValue(path, out var payload) switch
        {
            true => payload.PathDetail,
            false => StatusCode.NotFound,
        };
    }

    public Option<string> Set(string path, DataETag data, string? leaseId, ScopeContext context)
    {
        context = context.With(_logger);

        if (!FileStoreTool.IsPathValid(path)) return (StatusCode.BadRequest, "Path is invalid");

        lock (_lock)
        {
            if (IsLeased(path, leaseId)) return (StatusCode.Conflict, "Path is leased");

            var result = _store.AddOrUpdate(path, x => new DirectoryDetail(data.ConvertTo(x), data, null), (x, current) =>
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

                return payload;
            });

            context.LogTrace("Set Path={path}, length={length}", path, data.Data.Length);
            return result.PathDetail.ETag;
        }
    }

    public IReadOnlyList<IStorePathDetail> Search(string pattern)
    {
        var query = QueryParameter.Parse(pattern).GetMatcher();

        var list = _store.Values
            .Select(x => x.PathDetail)
            .Where(x => pattern == "*" || query.IsMatch(x.Path, false))
            .ToImmutableArray();

        return list;
    }

    // ========================================================================
    // Lease

    public Option<LeaseRecord> AcquireLease(string path, TimeSpan leaseDuration, ScopeContext context)
    {
        context = context.With(_logger);

        lock (_lock)
        {
            if (!_store.TryGetValue(path, out var payload)) return StatusCode.NotFound;

            Option result = payload.LeaseRecord switch
            {
                LeaseRecord v when v.IsLeaseValid() == true => StatusCode.Conflict,
                LeaseRecord v => _leaseStore.TryRemove(v.LeaseId, out _) ? StatusCode.OK : StatusCode.Conflict,
                _ => StatusCode.OK,
            };

            if (result.IsError()) return result.LogStatus(context, "Failed to acquire lease").ToOptionStatus<LeaseRecord>();

            LeaseRecord leaseRecord = new(path, leaseDuration);
            var newPayload = payload with { LeaseRecord = leaseRecord };

            _store[path] = newPayload;
            _leaseStore[newPayload.LeaseRecord.LeaseId] = leaseRecord;

            context.LogTrace("Acquire lease Path={path}, leaseId={leaseId}", path, newPayload.LeaseRecord.LeaseId);
            return newPayload.LeaseRecord;
        }
    }

    public Option BreakLease(string path, ScopeContext context)
    {
        context = context.With(_logger);

        lock (_lock)
        {
            if (!_store.TryGetValue(path, out var payload)) return StatusCode.NotFound;

            if (payload.LeaseRecord != null)
            {
                _leaseStore.Remove(payload.LeaseRecord.LeaseId, out var _);
                _store[path] = payload with { LeaseRecord = null };
            }

            context.LogTrace("Break lease Path={path}");
            return StatusCode.OK;
        }
    }

    public Option ReleaseLease(string leaseId, ScopeContext context)
    {
        context = context.With(_logger);

        lock (_lock)
        {
            if (!_leaseStore.TryRemove(leaseId, out var leaseRecord)) return StatusCode.NotFound;

            _store[leaseRecord.Path] = _store[leaseRecord.Path] with { LeaseRecord = null };
            context.LogTrace("Release lease Path={path}, leaseId={leaseId}", leaseRecord.Path, leaseId);
            return StatusCode.OK;
        }
    }

    public bool IsLeased(string path, string? leaseId = null)
    {
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
}
