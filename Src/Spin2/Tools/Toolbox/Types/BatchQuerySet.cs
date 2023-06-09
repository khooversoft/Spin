using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Toolbox.Types;

public record BatchQuerySet<T>
{
    public QueryParameter QueryParameter { get; init; } = null!;
    public int NextIndex { get; init; }
    public IReadOnlyList<T> Items { get; init; } = null!;
    public bool IsEndSignaled { get => Items.Count == 0; }
}
