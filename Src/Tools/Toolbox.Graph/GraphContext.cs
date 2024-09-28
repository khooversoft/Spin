using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.TransactionLog;
using Toolbox.Types;

namespace Toolbox.Graph;

public interface IGraphContext
{
    GraphMap Map { get; }
    IFileStore FileStore { get; }
    ITransactionLog TransactionLog { get; }
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
    public ILogicalTrx LogicalTrx { get; }
}

public class GraphContext : IGraphContext
{
    private GraphMap _map = new GraphMap();
    public GraphContext(IFileStore fileStore, IGraphMapStore graphMapStore, ITransactionLog transactionLog
        )
    {
        FileStore = fileStore.NotNull();
        GraphMapStore = graphMapStore.NotNull();
        TransactionLog = transactionLog.NotNull();
    }

    public GraphMap Map => _map;
    public IFileStore FileStore { get; }
    public IGraphMapStore GraphMapStore { get; }
    public ITransactionLog TransactionLog { get; }

    public void SetMap(GraphMap map) => Interlocked.Exchange(ref _map, map.NotNull());

    public Task<Option> LoadMap(ScopeContext context) => GraphMapStore.Get(this, context);
    public Task<Option> CheckpointDatabaseMap(ScopeContext context) => GraphMapStore.Set(this, context);
}

public class GraphTrxContext : IGraphTrxContext
{
    private readonly IGraphContext _graphContext;
    private readonly ILogicalTrx _logicalTrx;

    public GraphTrxContext(IGraphContext graphContext, ILogicalTrx LogicalTrx, ScopeContext context)
    {
        _graphContext = graphContext.NotNull();
        _logicalTrx = LogicalTrx.NotNull();

        Context = context;
        ChangeLog = new ChangeLog(this);
    }

    public GraphMap Map => _graphContext.Map;
    public IFileStore FileStore => _graphContext.FileStore;
    public ILogicalTrx LogicalTrx => _logicalTrx;
    public ChangeLog ChangeLog { get; }
    public ScopeContext Context { get; }
}
