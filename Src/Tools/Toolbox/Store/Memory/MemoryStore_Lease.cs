using Microsoft.Extensions.Logging;
using Toolbox.Types;

namespace Toolbox.Store;

public partial class MemoryStore
{
    public Option<LeaseRecord> AcquireLease(string path, TimeSpan leaseDuration)
    {
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
                        _logger.LogError("Failed to acquire lease, path={path}, current leaseId={leaseId}", path, v.LeaseRecord?.LeaseId);
                        return v;
                    }

                    leaseRecord = new LeaseRecord(path, leaseDuration);
                    var dirDetail = v with { LeaseRecord = leaseRecord };
                    _leaseStore[leaseRecord.LeaseId] = leaseRecord;

                    _logger.LogDebug("Acquire lease Path={path}, leaseId={leaseId}", path, leaseRecord.LeaseId);
                    return dirDetail;
                });

            return new Option<LeaseRecord>(leaseRecord, result.StatusCode, result.Error);
        }
    }

    public Option BreakLease(string path)
    {
        path = RemoveForwardSlash(path);

        lock (_lock)
        {
            if (!_store.TryGetValue(path, out var payload)) return (StatusCode.NotFound, "Lease not found");

            if (payload.LeaseRecord != null)
            {
                _leaseStore.Remove(payload.LeaseRecord.LeaseId, out var _);
                _store[path] = payload with { LeaseRecord = null };
            }

            _logger.LogDebug("Break lease Path={path}", path);
            return StatusCode.OK;
        }
    }

    public Option ReleaseLease(string path, string leaseId)
    {
        path = RemoveForwardSlash(path);

        lock (_lock)
        {
            if (!_store.ContainsKey(path)) return (StatusCode.NotFound, "Path not found");
            if (!_leaseStore.TryRemove(leaseId, out var leaseRecord)) return (StatusCode.NotFound, "Lease not found");
            if (path != leaseRecord.Path) return (StatusCode.BadRequest, "LeaseId does not match path");

            _store[path] = _store[path] with { LeaseRecord = null };
            _logger.LogDebug("Release lease Path={path}, leaseId={leaseId}", leaseRecord.Path, leaseId);
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
}
