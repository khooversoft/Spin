using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Toolbox.Data;

public record GraphNodeAdd : IGraphQL
{
    public string Key { get; init; } = null!;
    public string? Tags { get; init; }
}
