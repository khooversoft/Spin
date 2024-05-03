using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

public interface IGraphContext
{
    GraphMap Map { get; }
    IChangeTrace? ChangeTrace { get; }
    IGraphCommand? Command { get; }
    IGraphEntity? Entity { get; }
    IFileStore? FileStore { get; }
    IGraphStore? GraphStore { get; }
    IGraphTrxContext CreateTrxContext();
}

public interface IGraphTrxContext : IGraphContext
{
    ChangeLog ChangeLog { get; }
}

public class GraphContext : IGraphContext
{
    public GraphContext(GraphMap map, IChangeTrace? changeTrace, IGraphCommand? command, IGraphEntity? entity, IFileStore? fileStore, IGraphStore? graphStore)
    {
        Map = map;
        ChangeTrace = changeTrace;
        Command = command;
        Entity = entity;
        FileStore = fileStore;
        GraphStore = graphStore;
    }

    public GraphMap Map { get; }
    public IChangeTrace? ChangeTrace { get; }
    public IGraphCommand? Command { get; }
    public IGraphEntity? Entity { get; init; }
    public IFileStore? FileStore { get; init; }
    public IGraphStore? GraphStore { get; init; }

    public IGraphTrxContext CreateTrxContext() => new GraphTrxContext
    {
        Map = Map,
        FileStore = FileStore,
        ChangeTrace = ChangeTrace,
        Command = Command,
        GraphStore = GraphStore,
        Entity = Entity,
        ChangeLog = new ChangeLog(this),
    };
}

public class GraphTrxContext : GraphContext, IGraphTrxContext
{
    public ChangeLog ChangeLog { get; init; } = null!;
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