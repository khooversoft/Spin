using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Block;

public class BlockStreamReader<T> where T : class
{
    protected readonly IEnumerable<BlockNode> _blockNodes;
    public BlockStreamReader(IEnumerable<BlockNode> blockNodes) => _blockNodes = blockNodes.NotNull();

    public Option<T> GetLatest() => _blockNodes.Select(x => x.DataBlock.ToObject<T>()).LastOrDefaultOption();
    public IReadOnlyList<T> List() => _blockNodes.Select(x => x.DataBlock.ToObject<T>()).ToArray();
}


public class BlockStream<T> : BlockStreamReader<T> where T : class
{
    private readonly BlockChain _blockChain;
    private readonly string _blockType;

    public BlockStream(IEnumerable<BlockNode> blockNodes, BlockChain blockChain, string blockType)
        : base(blockNodes)
    {
        _blockChain = blockChain.NotNull();
        _blockType = NameId.Verify(blockType);
    }

    public Option Add(DataBlock value) => _blockChain.Add(value);

    public DataBlock CreateDataBlock(T subject, string principalId) => subject.ToDataBlock(principalId: principalId, blockType: _blockType);
}
