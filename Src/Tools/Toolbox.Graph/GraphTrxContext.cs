using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.TransactionLog;
using Toolbox.Types;

namespace Toolbox.Graph;

public interface IGraphTrxContext
{
    GraphMap Map { get; }
    IFileStore FileStore { get; }
    ChangeLog ChangeLog { get; }
    ScopeContext Context { get; }
    ILogicalTrx LogicalTrx { get; }
}


public class GraphTrxContext : IGraphTrxContext
{
    private readonly IGraphHost _graphContext;

    public GraphTrxContext(IGraphHost graphContext, ILogicalTrx logicalTrx, ScopeContext context)
    {
        _graphContext = graphContext.NotNull();
        LogicalTrx = logicalTrx;
        Context = context;
        ChangeLog = new ChangeLog(this);
    }

    public GraphMap Map => _graphContext.Map;
    public IFileStore FileStore => _graphContext.FileStore;
    public ChangeLog ChangeLog { get; }
    public ScopeContext Context { get; }
    public ILogicalTrx LogicalTrx { get; }
}
