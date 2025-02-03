using Toolbox.Journal;
using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

public interface IGraphTrxContext : IAsyncDisposable
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


public class GraphTrxContext : IGraphTrxContext, IAsyncDisposable
{
    private readonly IGraphHost _graphHost;

    public GraphTrxContext(IGraphHost graphHost, ScopeContext context)
    {
        _graphHost = graphHost.NotNull();
        TransactionWriter = graphHost.TransactionLog.NotNull().CreateTransactionContext();
        TraceWriter = graphHost.TraceLog.NotNull().CreateTransactionContext();
        Context = context.With(_graphHost.Logger);
        ChangeLog = new ChangeLog(this);
    }

    public Task<Option> CheckpointMap(ScopeContext context) => _graphHost.CheckpointMap(context);
    public ScopeContext Context { get; }
    public ChangeLog ChangeLog { get; }
    public IFileStore FileStore => _graphHost.FileStore;
    public IJournalTrx TransactionWriter { get; }
    public IJournalTrx TraceWriter { get; }
    public Task<Option> LoadMap(ScopeContext context) => _graphHost.LoadMap(context);

    public async ValueTask DisposeAsync()
    {
        await TransactionWriter.DisposeAsync();
        await TraceWriter.DisposeAsync();
    }

    public GraphMap Map => _graphHost.Map;
}
