using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Store;

public sealed class InMemoryFileStore : IFileStore, IEnumerable<KeyValuePair<string, DataETag>>
{
    private readonly ConcurrentDictionary<string, DataETag> _store = new(StringComparer.OrdinalIgnoreCase);
    private readonly object _lock = new object();
    private readonly ILogger<InMemoryFileStore> _logger;

    public InMemoryFileStore(ILogger<InMemoryFileStore> logger) => _logger = logger.NotNull();

    public int Count => _store.Count;

    public Task<Option<string>> Add(string path, DataETag data, ScopeContext context)
    {
        if (!FileStoreTool.IsPathValid(path)) return new Option<string>(StatusCode.BadRequest).ToTaskResult();
        context = context.With(_logger);

        data = data.WithHash();

        lock (_lock)
        {
            if (data.ETag.IsNotEmpty())
            {
                if (_store.TryGetValue(path, out var current))
                {
                    if (current.ETag != data.ETag)
                    {
                        context.LogError("ETag for path={path} does not match, current.ETag={current.ETag}, data.ETag={data.ETag}", current.ETag, data.ETag);
                        return new Option<string>(StatusCode.Conflict, $"ETag does not match").ToTaskResult();
                    }
                }
            }

            context.LogInformation("Add Path={path} with eTag={eTag}", path, data.ETag);

            Option<string> option = _store.TryAdd(path, data) switch
            {
                true => data.ETag.NotEmpty(),
                false => (StatusCode.Conflict, $"path={path} already exist"),
            };

            option.LogStatus(context, "Add Path");
            return option.ToTaskResult();
        }
    }
    public Task<Option> Append(string path, DataETag data, ScopeContext context)
    {
        lock (_lock)
        {
            DataETag value = data;

            if (_store.TryGetValue(path, out DataETag readValue))
            {
                value = readValue.Data.Concat(data.Data).ToArray();
            };

            _store[path] = value;
            context.LogInformation("Append Path={path} with {length} bytes", path, data.Data.Length);
            return new Option(StatusCode.OK).ToTaskResult();
        }
    }

    public Task<Option> Delete(string path, ScopeContext context)
    {
        if (!FileStoreTool.IsPathValid(path)) return new Option(StatusCode.BadRequest).ToTaskResult();

        Option option = _store.TryRemove(path, out var _) switch
        {
            true => StatusCode.OK,
            false => (StatusCode.NotFound, $"path={path} does not exist"),
        };

        option.LogStatus(context, "Delete");
        return option.ToTaskResult();
    }

    public Task<Option> Exist(string path, ScopeContext context)
    {
        if (!FileStoreTool.IsPathValid(path)) return new Option(StatusCode.BadRequest).ToTaskResult();

        Option option = _store.ContainsKey(path) switch
        {
            true => StatusCode.OK,
            false => (StatusCode.NotFound, $"path={path} does not exist"),
        };

        return option.ToTaskResult();
    }

    public Task<Option<DataETag>> Get(string path, ScopeContext context)
    {
        Option<DataETag> option = _store.TryGetValue(path, out var value) switch
        {
            true => value,
            false => (StatusCode.NotFound, $"path={path} does not exist"),
        };

        option.LogStatus(context, "Get Path");
        return option.ToTaskResult();
    }

    public Task<IReadOnlyList<string>> Search(string pattern, ScopeContext context)
    {
        var query = QueryParameter.Parse(pattern).GetMatcher();

        if (pattern == "*") return ((IReadOnlyList<string>)_store.Keys).ToTaskResult();

        var paths = _store
            .Select(x => x.Key)
            .Where(x => query.IsMatch(x, false))
            .ToImmutableArray();

        return ((IReadOnlyList<string>)paths).ToTaskResult();
    }

    public Task<Option<string>> Set(string path, DataETag data, ScopeContext context)
    {
        lock (_lock)
        {
            if (data.ETag.IsNotEmpty())
            {
                if (_store.TryGetValue(path, out var current))
                {
                    if (current.ETag != data.ETag)
                    {
                        context.LogError("ETag for path={path} does not match, current.ETag={current.ETag}, data.ETag={data.ETag}", path, current.ETag, data.ETag);
                        return new Option<string>(StatusCode.Conflict, $"ETag does not match").ToTaskResult();
                    }
                }
            }

            data = data.WithHash();
            _store[path] = data;
            return data.ETag.NotEmpty().ToOption().ToTaskResult();
        }
    }

    public IEnumerator<KeyValuePair<string, DataETag>> GetEnumerator() => _store.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
