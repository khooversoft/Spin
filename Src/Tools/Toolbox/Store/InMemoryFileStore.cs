using System.Collections;
using System.Collections.Concurrent;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Store;

public class InMemoryFileStore : IFileStore, IEnumerable<KeyValuePair<string, DataETag>>
{
    private readonly ConcurrentDictionary<string, DataETag> _store = new(StringComparer.OrdinalIgnoreCase);
    private readonly object _lock = new object();

    public int Count => _store.Count;

    public Task<Option<string>> Add(string path, DataETag data, ScopeContext context)
    {
        data = data.WithHash();

        lock (_lock)
        {
            if (data.ETag.IsNotEmpty())
            {
                if (_store.TryGetValue(path, out var current))
                {
                    if (current.ETag != data.ETag) return new Option<string>(StatusCode.Conflict, $"ETag does not match").ToTaskResult();
                }
            }

            Option<string> option = _store.TryAdd(path, data) switch
            {
                true => data.ETag.NotEmpty(),
                false => (StatusCode.Conflict, $"path={path} already exist"),
            };

            return option.ToTaskResult();
        }
    }

    public Task<Option> Delete(string path, ScopeContext context)
    {
        Option option = _store.TryRemove(path, out var _) switch
        {
            true => StatusCode.OK,
            false => (StatusCode.NotFound, $"path={path} does not already exist"),
        };

        return option.ToTaskResult();
    }

    public Task<Option> Exist(string path, ScopeContext context)
    {
        Option option = _store.ContainsKey(path) switch
        {
            true => StatusCode.OK,
            false => (StatusCode.NotFound, $"path={path} does not already exist"),
        };

        return option.ToTaskResult();
    }

    public Task<Option<DataETag>> Get(string path, ScopeContext context)
    {
        Option<DataETag> option = _store.TryGetValue(path, out var value) switch
        {
            true => value,
            false => (StatusCode.NotFound, $"path={path} does not already exist"),
        };

        return option.ToTaskResult();
    }

    public Task<IReadOnlyList<string>> Search(string pattern, ScopeContext context)
    {
        var paths = _store.Select(x => x.Key);
        var result = paths.Match(pattern);
        return result.ToTaskResult();
    }

    public Task<Option<string>> Set(string path, DataETag data, ScopeContext context)
    {
        lock (_lock)
        {
            if (data.ETag.IsNotEmpty())
            {
                if (_store.TryGetValue(path, out var current))
                {
                    if (current.ETag != data.ETag) return new Option<string>(StatusCode.Conflict, $"ETag does not match").ToTaskResult();
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
