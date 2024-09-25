using Microsoft.Extensions.Logging.Abstractions;
using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;


public class GraphContext : IGraphContext
{
    public GraphContext(GraphMap map, IFileStore fileStore)
    {
        Map = map.NotNull();
        FileStore = fileStore.NotNull();
    }

    public GraphMap Map { get; private init; }
    public IFileStore FileStore { get; private init; }
}

public class GraphTrxContext : IGraphTrxContext
{
    public GraphTrxContext(GraphMap map, ScopeContext context)
    {
        Map = map.NotNull();
        FileStore = new InMemoryFileStore(NullLogger<InMemoryFileStore>.Instance);
        ChangeLog = new ChangeLog(this);
        Context = context;
    }

    public GraphTrxContext(GraphMap map, IFileStore fileStore, ScopeContext context)
    {
        Map = map.NotNull();
        FileStore = fileStore.NotNull();
        ChangeLog = new ChangeLog(this);
        Context = context;
    }

    public ChangeLog ChangeLog { get; }
    public IFileStore FileStore { get; }
    public GraphMap Map { get; }
    public ScopeContext Context { get; }
}

public static class GraphContextExtensions
{
    public static GraphTrxContext CreateTrxContext(this IGraphContext graphContext, ScopeContext context)
    {
        graphContext.NotNull();
        return new GraphTrxContext(graphContext.Map, graphContext.FileStore, context);
    }
}
