using System.Collections.Concurrent;
using System.Collections.Immutable;
using Toolbox.Extensions;
using Toolbox.Types;

namespace Toolbox.Store;

public sealed class MemoryStore
{
    private record DirectoryDetail(StorePathDetail PathDetail, DataETag Data, LeaseRecord? LeaseRecord = null);

    private readonly ConcurrentDictionary<string, DirectoryDetail> _store = new(StringComparer.OrdinalIgnoreCase);

    private readonly ConcurrentDictionary<string, LeaseRecord> _leaseStore = new(StringComparer.OrdinalIgnoreCase);
    private readonly object _lock = new object();

    public MemoryStore() { }

    public Option Add(string path, DataETag data)
    {
        if (!FileStoreTool.IsPathValid(path)) return (StatusCode.BadRequest, "Path is invalid");

        lock (_lock)
        {
            if (IsLeased(path)) return (StatusCode.Conflict, "Path is leased");

            var result = _store.TryAdd(path, new DirectoryDetail(data.ConvertTo(path), data, null)) switch
            {
                true => StatusCode.OK,
                false => StatusCode.Conflict,
            };

            return result;
        }
    }

    public Option Append(string path, DataETag data, string? leaseId = null)
    {
        lock (_lock)
        {
            if (IsLeased(path, leaseId)) return (StatusCode.Conflict, "Path is leased");

            var pathDetail = data.ConvertTo(path);

            if (_store.TryGetValue(path, out var readPayload))
            {
                readPayload = readPayload with { Data = readPayload.Data.Data.Concat(data.Data).ToDataETag() };
                _store[path] = readPayload;
                return StatusCode.OK;
            }

            _store[path] = new DirectoryDetail(pathDetail, data);
            return StatusCode.OK;
        }
    }

    public bool Exist(string path) => _store.ContainsKey(path);

    public Option<DataETag> Get(string path) => _store.TryGetValue(path, out var payload) switch
    {
        true => payload.Data,
        false => StatusCode.NotFound,
    };

    public Option Remove(string path, string? leaseId = null)
    {
        lock (_lock)
        {
            if (IsLeased(path, leaseId)) return (StatusCode.Conflict, "Path is leased");

            Option result = _store.TryRemove(path, out var payload) switch
            {
                true when payload.LeaseRecord != null => _leaseStore.TryRemove(payload.LeaseRecord.LeaseId, out _) ? StatusCode.OK : StatusCode.Conflict,
                true => StatusCode.OK,
                _ => StatusCode.NotFound,
            };

            return result;
        }
    }

    public Option Set(string path, DataETag data, string? leaseId = null)
    {
        if (!FileStoreTool.IsPathValid(path)) return (StatusCode.BadRequest, "Path is invalid");

        lock (_lock)
        {
            if (IsLeased(path, leaseId)) return (StatusCode.Conflict, "Path is leased");

            _store.AddOrUpdate(path, x => new DirectoryDetail(data.ConvertTo(x), data, null), (x, current) =>
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

            return StatusCode.OK;
        }
    }

    public IReadOnlyList<StorePathDetail> Search(string pattern)
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

    public Option<string> AcquireLease(string path, TimeSpan leaseDuration)
    {
        lock (_lock)
        {
            if (!_store.TryGetValue(path, out var payload)) return StatusCode.NotFound;

            Option result = payload.LeaseRecord switch
            {
                LeaseRecord v when v.IsLeaseValid() == true => StatusCode.Conflict,
                LeaseRecord v => _leaseStore.TryRemove(v.LeaseId, out _) ? StatusCode.OK : StatusCode.Conflict,
                _ => StatusCode.OK,
            };

            if (result.IsError()) return result.ToOptionStatus<string>();

            LeaseRecord leaseRecord = new(path, leaseDuration);
            var newPayload = payload with { LeaseRecord = leaseRecord };

            _store[path] = newPayload;
            _leaseStore[newPayload.LeaseRecord.LeaseId] = leaseRecord;

            return newPayload.LeaseRecord.LeaseId;
        }
    }

    public Option<string> BreakLease(string path, TimeSpan leaseDuration)
    {
        lock (_lock)
        {
            if (!_store.TryGetValue(path, out var payload)) return StatusCode.NotFound;

            payload.LeaseRecord?.Action(x => _leaseStore.Remove(x.LeaseId, out var _));

            LeaseRecord newLease = new(path, leaseDuration);
            _store[path] = payload with { LeaseRecord = newLease };

            return newLease.LeaseId;
        }
    }

    public Option ClearLease(string path)
    {
        lock (_lock)
        {
            if (!_store.TryGetValue(path, out var payload)) return StatusCode.NotFound;

            if (payload.LeaseRecord != null)
            {
                _leaseStore.Remove(payload.LeaseRecord.LeaseId, out var _);
                _store[path] = payload with { LeaseRecord = null };
            }

            return StatusCode.OK;
        }
    }

    public Option ReleaseLease(string leaseId)
    {
        lock (_lock)
        {
            if (!_leaseStore.TryRemove(leaseId, out var leaseRecord)) return StatusCode.NotFound;

            _store[leaseRecord.Path] = _store[leaseRecord.Path] with { LeaseRecord = null };
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
