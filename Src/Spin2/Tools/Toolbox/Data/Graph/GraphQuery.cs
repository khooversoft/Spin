using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Toolbox.Data.Graph;

public readonly record struct QueryContext
{
    [SetsRequiredMembers]
    public QueryContext() { }

    public GraphMap<string> Map { get; init; } = null!;
    public Stack<IGraphCommon> Stack { get; init; } = new Stack<IGraphCommon>();
    public Dictionary<string, IGraphCommon> Alias { get; init; } = new Dictionary<string, IGraphCommon>(StringComparer.OrdinalIgnoreCase);
}

public class GraphQuery
{
}
