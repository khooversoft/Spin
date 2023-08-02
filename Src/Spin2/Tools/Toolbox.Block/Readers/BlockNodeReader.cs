using Toolbox.Tools;

namespace Toolbox.Block;

public class BlockNodeReader
{
    protected readonly IReadOnlyList<BlockNode> _blockNodes;
    internal BlockNodeReader(IReadOnlyList<BlockNode> blockNodes) => _blockNodes = blockNodes.NotNull();

    public BlockNode this[int index] => _blockNodes[index];
    public int Count => _blockNodes.Count;

    public IReadOnlyList<BlockNode> Items => _blockNodes.ToArray();
}
