using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Block;
using Toolbox.Tools;
using Toolbox.Types;

public class BlockReader<T> where T : class
{
    protected readonly IEnumerable<T> _blockNodes;
    internal BlockReader(IEnumerable<T> blockNodes) => _blockNodes = blockNodes.NotNull();

    public T this[int index] => _blockNodes.ToArray()[index];
    public int Count => _blockNodes.Count();
    public Option<T> GetLatest() => _blockNodes.LastOrDefaultOption();
    public IReadOnlyList<T> List() => _blockNodes.ToArray();
}


public class BlockWriter<T> where T : class
{
    private readonly BlockChain _blockChain;
    private readonly string _blockType;

    internal BlockWriter(BlockChain blockChain, string blockType)
    {
        _blockChain = blockChain.NotNull();
        _blockType = NameId.Create(blockType).ThrowOnError().Return();
    }

    public Option Add(DataBlock value) => _blockChain.Add(value);

    public DataBlock CreateDataBlock(T subject, string principalId) => subject.ToDataBlock(principalId: principalId, blockType: _blockType);
}
