using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

public class GraphAccess
{
    private GraphMap _map = new GraphMap();
    private readonly IGraphStore _graphStore;

    public GraphAccess(IGraphStore graphStore) => _graphStore = graphStore.NotNull();

    public Task Clear() => Task.CompletedTask.Action(x => _map = new GraphMap());

    public async Task<Option<GraphQueryResults>> Execute(string graphQuery, ScopeContext context)
    {
        var result = await GraphCommand.Execute(_map, graphQuery, _graphStore, context);
        return result;
    }

    public async Task<Option<GraphQueryResult>> ExecuteScalar(string graphQuery, ScopeContext context)
    {
        var result = await GraphCommand.Execute(_map, graphQuery, _graphStore, context);
        if (result.IsError()) return result.ToOptionStatus<GraphQueryResult>();

        return result.Return().Items.First();
    }

    public async Task<Option> Read(ScopeContext context)
    {
        var gsOption = await _graphStore.Get<GraphSerialization>("directory.json", context);
        if (gsOption.IsError()) return gsOption.ToOptionStatus();

        _map = gsOption.Return().FromSerialization();
        return StatusCode.OK;
    }

    public async Task<Option> Write(ScopeContext context)
    {
        var gs = _map.ToSerialization();
        return await _graphStore.Set("directory.json", gs, context);
    }
}