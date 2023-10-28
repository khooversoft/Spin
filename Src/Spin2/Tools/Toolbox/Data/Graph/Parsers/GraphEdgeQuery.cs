using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Toolbox.Data;

public readonly record struct GraphEdgeQuery<TKey> : IGraphQL where TKey : notnull
{
    public GraphEdgeQuery() { }

    public TKey? NodeKey { get; init; }
    public TKey? FromKey { get; init; }
    public TKey? ToKey { get; init; }
    public string? EdgeType { get; init; }
    public string? Tags { get; init; }
    public EdgeDirection Direction { get; init; } = EdgeDirection.Both;
}
