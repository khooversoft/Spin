using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

public class GraphContext
{
    public GraphContext(GraphMap map, ScopeContext context)
    {
        Map = map.NotNull();
        ChangeLog = new ChangeLog(this);
        Context = context.NotNull();
    }

    public GraphContext(GraphMap map, IFileStore store, IChangeTrace changeTrace, ScopeContext context)
    {
        Map = map.NotNull();
        ChangeLog = new ChangeLog(this);
        Store = store.NotNull();
        ChangeTrace = changeTrace.NotNull();
        Context = context.NotNull();
    }

    public GraphMap Map { get; }
    public ChangeLog ChangeLog { get; }
    public IFileStore? Store { get; }
    public IChangeTrace? ChangeTrace { get; }
    public ScopeContext Context { get; }
}


public static class GraphContextExtensions
{
    public static async Task<Option<GraphQueryResults>> Execute(this GraphContext graphContext, string graphQuery)
    {
        var result = await GraphCommand.Execute(graphContext, graphQuery);
        return result;
    }

    public static async Task<Option<GraphQueryResult>> ExecuteScalar(this GraphContext graphContext, string graphQuery)
    {
        var result = await GraphCommand.Execute(graphContext, graphQuery);
        if (result.IsError()) return result.ToOptionStatus<GraphQueryResult>();

        return result.Return().Items.First();
    }
}