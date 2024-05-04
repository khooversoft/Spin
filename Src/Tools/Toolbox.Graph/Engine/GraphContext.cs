using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;


public class GraphContext : IGraphContext
{
    public GraphContext(GraphMap map, IChangeTrace? changeTrace, IFileStore? fileStore)
    {
        Map = map.NotNull();
        ChangeTrace = changeTrace.NotNull();
        FileStore = fileStore.NotNull();
    }

    public GraphMap Map { get; private init; }
    public IChangeTrace ChangeTrace { get; private init; }
    public IFileStore FileStore { get; private init; }

    public IGraphTrxContext CreateTrxContext() => new GraphTrxContext(
        Map,
        ChangeTrace,
        FileStore
    );
}

public class GraphTrxContext : GraphContext, IGraphTrxContext
{
    public GraphTrxContext(GraphMap map, IChangeTrace changeTrace, IFileStore fileStore)
        : base(map, changeTrace, fileStore)
    {
        ChangeLog = new ChangeLog(this);
    }

    public ChangeLog ChangeLog { get; }
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