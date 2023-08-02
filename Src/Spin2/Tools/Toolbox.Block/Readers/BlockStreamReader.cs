using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Block;

public class BlockStreamReader<T> where T : class
{
    protected readonly IEnumerable<BlockNode> _blockNodes;
    internal BlockStreamReader(IEnumerable<BlockNode> blockNodes) => _blockNodes = blockNodes.NotNull();

    public Option<T> GetLatest() => _blockNodes.Select(x => x.DataBlock.ToObject<T>()).LastOrDefaultOption();
    public IReadOnlyList<T> List() => _blockNodes.Select(x => x.DataBlock.ToObject<T>()).ToArray();
}
