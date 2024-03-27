using System.Collections;
using System.Collections.Concurrent;
using System.Text;
using Azure;
using Toolbox.Extensions;
using Toolbox.Tools;
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

    public Task<Option> Add<T>(string path, T value, ScopeContext context) where T : class
    {
        string json = value.ToJson();
        DataETag data = new DataETag(json.ToBytes());
        return Add(path, data, context);
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

    public async Task<Option<T>> Get<T>(string path, ScopeContext context)
    {
        Option<T> option = (await Get(path, context)) switch
        {
            var v when v.IsOk() => v.Return().Data.AsSpan().ToObject<T>().NotNull(),
            var v => v.ToOptionStatus<T>(),
        };

        return option;
    }

    public Task<Option> Set(string path, DataETag data, ScopeContext context)
    {
        _store[path] = data;
        return new Option(StatusCode.OK).ToTaskResult();
    }

    public Task<Option> Set<T>(string path, T value, ScopeContext context) where T : class
    {
        string json = value.ToJson();
        DataETag data = new DataETag(Encoding.UTF8.GetBytes(json));
        return Set(path, data, context);
    }

    public IEnumerator<KeyValuePair<string, DataETag>> GetEnumerator() => _store.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

