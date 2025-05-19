using Toolbox.Journal;
using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

public interface IGraphTrxContext : IAsyncDisposable
{
    ScopeContext Context { get; }
    ChangeLog ChangeLog { get; }
    IFileStore FileStore { get; }
    IJournalTrx TransactionWriter { get; }
    GraphMap Map { get; }

    Task<Option<IFileLeasedAccess>> AcquireLease();
    Task<Option> CheckpointMap();
}


public class GraphTrxContext : IGraphTrxContext, IAsyncDisposable
{
    private readonly IGraphEngine _graphHost;

    public GraphTrxContext(IGraphEngine graphHost, ScopeContext context)
    {
        _graphHost = graphHost.NotNull();
        Context = context;

        TransactionWriter = graphHost.TransactionLog.NotNull().CreateTransactionContext();
        ChangeLog = new ChangeLog(this);
    }

    public GraphMap Map => _graphHost.Map;

    public ScopeContext Context { get; }
    public ChangeLog ChangeLog { get; }
    public IFileStore FileStore => _graphHost.FileStore;
    public IJournalTrx TransactionWriter { get; }

    public Task<Option<IFileLeasedAccess>> AcquireLease() => _graphHost.AcquireLease(Context);
    public Task<Option> CheckpointMap() => _graphHost.CheckpointMap(Context);

    public async ValueTask DisposeAsync() => await TransactionWriter.DisposeAsync();
}
