using Toolbox.Journal;
using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

public interface IGraphTrxContext : IAsyncDisposable
{
    GraphMap Map { get; }

    ScopeContext Context { get; }
    ChangeLog ChangeLog { get; }
    IGraphEngine GraphEngine { get; }
    IFileStore FileStore { get; }
    IJournalTrx TransactionWriter { get; }
}


public class GraphTrxContext : IGraphTrxContext, IAsyncDisposable
{
    private readonly IGraphEngine _graphEngine;

    public GraphTrxContext(IGraphEngine graphEngine, ScopeContext context)
    {
        _graphEngine = graphEngine.NotNull();
        Context = context;

        TransactionWriter = graphEngine.TransactionLog.NotNull().CreateTransactionContext();
        ChangeLog = new ChangeLog(this);
    }

    public GraphMap Map => _graphEngine.GetMapData().NotNull().Map;

    public ScopeContext Context { get; }
    public ChangeLog ChangeLog { get; }
    public IGraphEngine GraphEngine => _graphEngine;
    public IFileStore FileStore => _graphEngine.FileStore;
    public IJournalTrx TransactionWriter { get; }

    public async ValueTask DisposeAsync() => await TransactionWriter.DisposeAsync();
}
