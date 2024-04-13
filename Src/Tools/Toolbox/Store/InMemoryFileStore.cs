using System.Collections;
using System.Collections.Concurrent;
using Toolbox.Extensions;
using Toolbox.Types;

namespace Toolbox.Store;

public class InMemoryFileStore : IFileStore, IEnumerable<KeyValuePair<string, DataETag>>
{
    private readonly ConcurrentDictionary<string, DataETag> _store = new(StringComparer.OrdinalIgnoreCase);

    public int Count => _store.Count;

    public Task<Option> Add(string path, DataETag data, ScopeContext context)
    {
        Option option = _store.TryAdd(path, data) switch
        {
            true => StatusCode.OK,
            false => (StatusCode.Conflict, $"path={path} already exist"),
        };

        return option.ToTaskResult();
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

    public Task<Option> Set(string path, DataETag data, ScopeContext context)
    {
        _store[path] = data;
        return new Option(StatusCode.OK).ToTaskResult();
    }

    public IEnumerator<KeyValuePair<string, DataETag>> GetEnumerator() => _store.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

