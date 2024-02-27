using Toolbox.Types;

namespace Toolbox.Data;

public record GraphNodeAdd : IGraphQL
{
    public string Key { get; init; } = null!;
    public Tags Tags { get; init; } = new Tags();
}
