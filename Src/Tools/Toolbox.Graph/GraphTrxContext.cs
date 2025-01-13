using Toolbox.Journal;
using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

public interface IGraphTrxContext
{
    Task<Option> CheckpointMap(ScopeContext context);
    ScopeContext Context { get; }
    ChangeLog ChangeLog { get; }
    IFileStore FileStore { get; }
    IJournalTrx TransactionWriter { get; }
    IJournalTrx TraceWriter { get; }
    Task<Option> LoadMap(ScopeContext context);
    GraphMap Map { get; }
}


public class GraphTrxContext : IGraphTrxContext
{
    private readonly IGraphHost _graphHost;

    public GraphTrxContext(IGraphHost grapHost, IJournalFile transactionWriter, IJournalFile traceWriter, ScopeContext context)
    {
        _graphHost = grapHost.NotNull();
        TransactionWriter = transactionWriter.NotNull().CreateTransactionContext();
        TraceWriter = traceWriter.NotNull().CreateTransactionContext();
        Context = context;
        ChangeLog = new ChangeLog(this);
    }

    public Task<Option> CheckpointMap(ScopeContext context) => _graphHost.CheckpointMap(context);
    public ScopeContext Context { get; }
    public ChangeLog ChangeLog { get; }
    public IFileStore FileStore => _graphHost.FileStore;
    public IJournalTrx TransactionWriter { get; }
    public IJournalTrx TraceWriter { get; }
    public Task<Option> LoadMap(ScopeContext context) => _graphHost.LoadMap(context);
    public GraphMap Map => _graphHost.Map;
}
