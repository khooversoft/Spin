using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

public class GraphChangeContext
{
    public GraphChangeContext(GraphMap map, ChangeLog changeLog, IGraphStore? store, ScopeContext context)
    {
        Map = map.NotNull();
        ChangeLog = changeLog.NotNull();
        Store = store;
        Context = context.NotNull();
    }

    public GraphMap Map { get; }
    public ChangeLog ChangeLog { get; }
    public IGraphStore? Store { get; }
    public ScopeContext Context { get; }
}
