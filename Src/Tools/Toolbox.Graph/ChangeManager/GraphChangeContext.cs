using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

public record GraphChangeContext
{
    public GraphChangeContext(GraphMap map, ChangeLog changeLog, ScopeContext context)
    {
        Map = map.NotNull();
        ChangeLog = changeLog.NotNull();
        Context = context.NotNull();
    }

    public GraphMap Map { get; }
    public ChangeLog ChangeLog { get; }
    public ScopeContext Context { get; }
}
