using System.Collections.Concurrent;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

public class GraphMemoryStore : IGraphStore
{
    private ConcurrentDictionary<string, object> _store = new ConcurrentDictionary<string, object>();

    public Task<Option> Add<T>(string nodeKey, T node, ScopeContext context) where T : class
    {
        Option option = _store.TryAdd(nodeKey, node) switch
        {
            true => StatusCode.OK,
            false => (StatusCode.Conflict, $"Node key={nodeKey} already exist"),
        };

        return option.ToTaskResult();

    }

    public Task<Option> Delete(string nodeKey, ScopeContext context)
    {
        Option option = _store.TryRemove(nodeKey, out var _) switch
        {
            true => StatusCode.OK,
            false => (StatusCode.NotFound, $"Node key={nodeKey} not found"),
        };

        return option.ToTaskResult();
    }

    public Task<Option> Exist(string nodeKey, ScopeContext context)
    {
        Option option = _store.ContainsKey(nodeKey) switch
        {
            true => StatusCode.OK,
            false => StatusCode.NotFound,
        };
        
        return option.ToTaskResult();
    }

    public Task<Option<T>> Get<T>(string nodeKey, ScopeContext context)
    {
        Option<T> option = _store.TryGetValue(nodeKey, out var value) switch
        {
            true => (T)value,
            false => StatusCode.NotFound,
        };

        return option.ToTaskResult();
    }

    public Task<Option> Set<T>(string nodeKey, T node, ScopeContext context) where T : class
    {
        _store[nodeKey.NotEmpty()] = node;
        return new Option(StatusCode.OK).ToTaskResult();
    }
}
