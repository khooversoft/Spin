using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;


public class GraphContext : IGraphContext
{
    public GraphContext(GraphMap map, IChangeTrace changeTrace, IFileStore fileStore)
    {
        Map = map.NotNull();
        ChangeTrace = changeTrace.NotNull();
        FileStore = fileStore.NotNull();
    }

    public GraphMap Map { get; private init; }
    public IChangeTrace ChangeTrace { get; private init; }
    public IFileStore FileStore { get; private init; }
}

public class GraphTrxContext : IGraphTrxContext
{
    public GraphTrxContext(GraphMap map, ScopeContext context)
    {
        Map = map.NotNull();
        ChangeTrace = new InMemoryChangeTrace();
        FileStore = new InMemoryFileStore(NullLogger<InMemoryFileStore>.Instance);
        ChangeLog = new ChangeLog(this);
        Context = context;
    }

    public GraphTrxContext(GraphMap map, IChangeTrace changeTrace, IFileStore fileStore, ScopeContext context)
    {
        Map = map.NotNull();
        ChangeTrace = changeTrace.NotNull();
        FileStore = fileStore.NotNull();
        ChangeLog = new ChangeLog(this);
        Context = context;
    }

    public ChangeLog ChangeLog { get; }
    public IChangeTrace ChangeTrace { get; }
    public IFileStore FileStore { get; }
    public GraphMap Map { get; }
    public ScopeContext Context { get; }
}

public static class GraphContextExtensions
{
    public static GraphTrxContext CreateTrxContext(this IGraphContext graphContext, ScopeContext context)
    {
        graphContext.NotNull();
        return new GraphTrxContext(graphContext.Map, graphContext.ChangeTrace, graphContext.FileStore, context);
    }
}


//public static class GraphContextExtensions
//{
//    public static async Task<Option<GraphQueryResults>> Execute(this GraphContext graphContext, string graphQuery)
//    {
//        var result = await GraphCommand.Execute(graphContext, graphQuery);
//        return result;
//    }

//    public static async Task<Option<GraphQueryResult>> ExecuteScalar(this GraphContext graphContext, string graphQuery)
//    {
//        var result = await GraphCommand.Execute(graphContext, graphQuery);
//        if (result.IsError()) return result.ToOptionStatus<GraphQueryResult>();

//        return result.Return().Items.First();
//    }
//}