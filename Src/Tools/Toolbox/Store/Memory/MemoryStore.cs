using System.Collections.Concurrent;
using System.Collections.Immutable;
using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Store;

public partial class MemoryStore
{
    private readonly ConcurrentDictionary<string, DirectoryDetail> _store = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, LeaseRecord> _leaseStore = new(StringComparer.OrdinalIgnoreCase);
    private readonly object _lock = new object();
    private readonly ILogger<MemoryStore> _logger;

    public MemoryStore(ILogger<MemoryStore> logger) => _logger = logger.NotNull();

    public MemoryStore(IServiceProvider serviceProvider, ILogger<MemoryStore> logger)
    {
        _logger = logger.NotNull();
        serviceProvider.NotNull();
    }

    public Option<string> Add(string path, DataETag data)
    {
        if (path.IsEmpty()) return (StatusCode.BadRequest, "Path is required");
        path = StorePathTool.RemoveForwardSlash(path);
        _logger.LogDebug("Adding path={path}", path);
        data = data.WithHash();

        if (!PathValidator.IsPathValid(path)) return (StatusCode.BadRequest, "Path is invalid");

        lock (_lock)
        {
            if (IsLeased(path).IsLocked()) return (StatusCode.Conflict, "Path is leased");

            DirectoryDetail detail = new(data.ConvertTo(path), data, null);

            Option<string> result = _store.TryAdd(path, detail) switch
            {
                true => detail.PathDetail.ETag.NotEmpty().ToOption(),
                false => StatusCode.Conflict,
            };

            if (result.IsOk()) _recorder?.Add(path, detail);

            _logger.LogDebug("Add Path={path}, length={length}", path, data.Data.Length);
            return result;
        }
    }

    public Option<string> Append(string path, DataETag data, string? leaseId)
    {
        _logger.LogDebug("Appending path={path}, leaseId={leaseId}", path, leaseId);
        path = StorePathTool.RemoveForwardSlash(path);
        data = data.WithHash();

        lock (_lock)
        {
            if (IsLeased(path, leaseId).IsLocked()) return (StatusCode.Conflict, "Path is leased");

            StorePathDetail pathDetail = data.ConvertTo(path);

            if (_store.TryGetValue(path, out var readPayload))
            {
                var newPayload = readPayload with { Data = (readPayload.Data + data).WithHash() };
                _store[path] = newPayload;

                _logger.LogDebug("Append Path={path}, length={length}", path, data.Data.Length);
                return newPayload.Data.ETag.NotEmpty();
            }

            DirectoryDetail detail = new(pathDetail, data);
            _store[path] = detail;
            return detail.Data.ETag.NotEmpty();
        }
    }

    public Option DeleteFolder(string pattern)
    {
        pattern = StorePathTool.AddRecursiveSafe(pattern);
        var matcher = new GlobFileMatching(pattern.NotEmpty());

        var result = _store.Keys
            .Where(x => matcher.IsMatch(x))
            .Select(x => Delete(x, null))
            .ToList();

        return result.Count > 0 ? StatusCode.OK : StatusCode.NotFound;
    }

    public bool Exist(string path) => _store.ContainsKey(StorePathTool.RemoveForwardSlash(path));

    public Option<DataETag> Get(string path) => _store.TryGetValue(StorePathTool.RemoveForwardSlash(path), out var payload) switch
    {
        true => payload.Data,
        false => StatusCode.NotFound,
    };

    public Option Delete(string path, string? leaseId)
    {
        path = StorePathTool.RemoveForwardSlash(path);

        lock (_lock)
        {
            if (IsLeased(path, leaseId).IsLocked()) return (StatusCode.Locked, "Path is leased");

            Option result = _store.TryRemove(path, out var payload) switch
            {
                true when payload.LeaseRecord != null => _leaseStore.TryRemove(payload.LeaseRecord.LeaseId, out _) ? StatusCode.OK : StatusCode.Conflict,
                true => StatusCode.OK,
                _ => StatusCode.NotFound,
            };

            if (result.IsOk()) _recorder?.Delete(path, payload!);

            _logger.LogTrace("Remove Path={path}, leaseId={leaseId}, statusCode={statusCode}, error={error}", path, leaseId ?? "<no leaseId>", result.StatusCode, result.Error);
            return result;
        }
    }

    public Option<StorePathDetail> GetDetail(string path)
    {
        path = StorePathTool.RemoveForwardSlash(path);

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

    public Option<string> Set(string path, DataETag data, string? leaseId)
    {
        path = StorePathTool.RemoveForwardSlash(path);
        if (!PathValidator.IsPathValid(path)) return (StatusCode.BadRequest, "Path is invalid");

        lock (_lock)
        {
            if (IsLeased(path, leaseId).IsLocked()) return (StatusCode.Locked, "Path is leased");

            if (data.ETag.IsNotEmpty())
            {
                if (_store.TryGetValue(path, out var existing))
                {
                    if (existing.Data.ETag != data.ETag) return (StatusCode.Conflict, "ETag does not match");
                }
            }

            data = data.WithHash();

            var result = _store.AddOrUpdate(path,
                x =>
                {
                    var p = new DirectoryDetail(data.ConvertTo(x), data, null);
                    _recorder?.Add(x, p);
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
                            ETag = data.ETag.NotEmpty(),
                        },
                    };

                    _recorder?.Update(x, current, payload);
                    return payload;
                });

            _logger.LogDebug("Set Path={path}, length={length}", path, data.Data.Length);
            return result.PathDetail.ETag;
        }
    }

    public IReadOnlyList<StorePathDetail> Search(string pattern, int index = 0, int size = -1)
    {
        pattern = StorePathTool.RemoveForwardSlash(pattern);
        index.Assert(x => x >= 0, "Index must be greater than or equal to zero");
        size.Assert(x => x == -1 || x > 0, "Size must be greater than zero or -1 for unlimited");

        var query = new GlobFileMatching(pattern);
        int maxSize = size < 1 ? int.MaxValue : size;

        var list = _store.Values
            .Where(x => pattern == "*" || query.IsMatch(x.PathDetail.Path))
            .Select(x => x.PathDetail with { Path = StorePathTool.RemoveForwardSlash(x.PathDetail.Path) })
            .OrderBy(x => x.Path)
            .Skip(index)
            .Take(maxSize)
            .ToImmutableArray();

        return list;
    }

    public IReadOnlyList<(StorePathDetail Detail, DataETag Data)> SearchData(string pattern)
    {
        var query = new GlobFileMatching(pattern);

        var list = _store.Values
            .Select(x => (Detail: x.PathDetail, Data: x.Data))
            .Where(x => pattern == "*" || query.IsMatch(x.Detail.Path))
            .Where(x => pattern == "*" || query.IsMatch(x.Detail.Path))
            .ToImmutableArray();

        return list;
    }
}
