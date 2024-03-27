using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

public class GraphChangeContext
{
    public GraphChangeContext(GraphMap map, ChangeLog changeLog, IFileStore? store, ScopeContext context)
    {
        Map = map.NotNull();
        ChangeLog = changeLog.NotNull();
        Store = store;
        Context = context.NotNull();
    }

    public GraphMap Map { get; }
    public ChangeLog ChangeLog { get; }
    public IFileStore? Store { get; }
    public ScopeContext Context { get; }
}
