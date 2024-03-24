using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

public class GraphStoreAccess : IGraphStore
{
    private readonly IGraphStore _store;
    private readonly GraphAccess _graphAccess;

    public GraphStoreAccess(IGraphStore store, GraphAccess graphAccess)
    {
        _store = store.NotNull();
        _graphAccess = graphAccess.NotNull();
    }

    public async Task<Option> Add<T>(string nodeKey, T node, ScopeContext context) where T : class
    {
        var query = await _graphAccess.ExecuteScalar($"(key={CreateNodePath(nodeKey)});", context);
        if( query.IsError()) return (StatusCode.NotFound, $"NodeKey={nodeKey} not found");

        nodeKey = CreateNodePath(nodeKey);
        return await _store.Add<T>(nodeKey, node, context);
    }

    public Task<Option> Delete(string nodeKey, ScopeContext context)
    {
        nodeKey = CreateNodePath(nodeKey);
        return _store.Delete(nodeKey, context);
    }

    public Task<Option> Exist(string nodeKey, ScopeContext context)
    {
        nodeKey = CreateNodePath(nodeKey);
        return _store.Exist(nodeKey, context);
    }

    public Task<Option<T>> Get<T>(string nodeKey, ScopeContext context)
    {
        nodeKey = CreateNodePath(nodeKey);
        return _store.Get<T>(nodeKey, context);
    }

    public Task<Option> Set<T>(string nodeKey, T node, ScopeContext context) where T : class
    {
        nodeKey = CreateNodePath(nodeKey);
        return _store.Set<T>(nodeKey, node, context);
    }

    public static string CreateNodePath(string path)
    {
        string file = path.NotEmpty().Replace('/', '_');
        string storePath = path.Split(new char[] { ':', '/' }, StringSplitOptions.RemoveEmptyEntries).Join('/');

        return $"nodes/{storePath}/{file}";
    }
}
