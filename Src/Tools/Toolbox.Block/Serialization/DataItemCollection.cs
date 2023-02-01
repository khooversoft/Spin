using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Toolbox.Block.Serialization;

public record DataItemCollection<T> where T : class
{
    public IReadOnlyList<T> Items { get; init; } = Array.Empty<T>();
}
