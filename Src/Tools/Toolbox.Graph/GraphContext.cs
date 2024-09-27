using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

public interface IGraphContext
{
    GraphMap Map { get; }
    IFileStore FileStore { get; }
    void SetMap(GraphMap map);
    Task<Option> LoadMap(ScopeContext context);
    Task<Option> CheckpointDatabaseMap(ScopeContext context);
}

public interface IGraphTrxContext
{
    GraphMap Map { get; }
    IFileStore FileStore { get; }
    ChangeLog ChangeLog { get; }
    ScopeContext Context { get; }
}

public class GraphContext : IGraphContext
{
    private GraphMap _map = new GraphMap();
    public GraphContext(IFileStore fileStore, IGraphMapStore graphMapStore)
    {
        FileStore = fileStore.NotNull();
        GraphMapStore = graphMapStore.NotNull();
    }

    public GraphMap Map => _map;
    public IFileStore FileStore { get; }
    public IGraphMapStore GraphMapStore { get; }

    public void SetMap(GraphMap map) => Interlocked.Exchange(ref _map, map.NotNull());

    public Task<Option> LoadMap(ScopeContext context) => GraphMapStore.Get(this, context);
    public Task<Option> CheckpointDatabaseMap(ScopeContext context) => GraphMapStore.Set(this, context);
}

public class GraphTrxContext : IGraphTrxContext
{
    private readonly IGraphContext _graphContext;

    public GraphTrxContext(IGraphContext graphContext, ScopeContext context)
    {
        _graphContext = graphContext.NotNull();

        Context = context;
        ChangeLog = new ChangeLog(this);
    }

    public GraphMap Map => _graphContext.Map;
    public IFileStore FileStore => _graphContext.FileStore;
    public ChangeLog ChangeLog { get; }
    public ScopeContext Context { get; }
}
