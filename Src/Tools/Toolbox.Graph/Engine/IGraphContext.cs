using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

public interface IGraphContext
{
    GraphMap Map { get; }
    IGraphFileStore FileStore { get; }
}

public interface IGraphTrxContext : IGraphContext
{
    ChangeLog ChangeLog { get; }
    ScopeContext Context { get; }
}