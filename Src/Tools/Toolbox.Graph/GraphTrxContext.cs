using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.TransactionLog;
using Toolbox.Types;

namespace Toolbox.Graph;

public interface IGraphTrxContext
{
    Task<Option> CheckpointMap(ScopeContext context);
    ScopeContext Context { get; }
    ChangeLog ChangeLog { get; }
    IFileStore FileStore { get; }
    ILogicalTrx LogicalTrx { get; }
    Task<Option> LoadMap(ScopeContext context);
    GraphMap Map { get; }
}


public class GraphTrxContext : IGraphTrxContext
{
    private readonly IGraphHost _graphHost;

    public GraphTrxContext(IGraphHost grapHost, ILogicalTrx logicalTrx, ScopeContext context)
    {
        _graphHost = grapHost.NotNull();
        LogicalTrx = logicalTrx.NotNull();
        Context = context;
        ChangeLog = new ChangeLog(this);
    }

    public Task<Option> CheckpointMap(ScopeContext context) => _graphHost.CheckpointMap(context);
    public ScopeContext Context { get; }
    public ChangeLog ChangeLog { get; }
    public IFileStore FileStore => _graphHost.FileStore;
    public ILogicalTrx LogicalTrx { get; }
    public Task<Option> LoadMap(ScopeContext context) => _graphHost.LoadMap(context);
    public GraphMap Map => _graphHost.Map;
}
