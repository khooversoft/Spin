using Toolbox.Store;
using Toolbox.Types;

namespace Toolbox.Graph;

public interface IGraphContext
{
    GraphMap Map { get; }
    IFileStore FileStore { get; }
}

public interface IGraphTrxContext : IGraphContext
{
    ChangeLog ChangeLog { get; }
    ScopeContext Context { get; }
}