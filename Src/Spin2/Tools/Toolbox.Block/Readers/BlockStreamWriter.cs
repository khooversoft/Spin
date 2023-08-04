//using Toolbox.Tools;
//using Toolbox.Types;

//namespace Toolbox.Block;

//public class BlockStreamWriter<T> where T : class
//{
//    private readonly BlockChain _blockChain;
//    private readonly string _blockType;

//    internal BlockStreamWriter(BlockChain blockChain, string blockType)
//    {
//        _blockChain = blockChain.NotNull();
//        _blockType = NameId.Verify(blockType);
//    }

//    public Option Add(DataBlock value) => _blockChain.Add(value);

//    public DataBlock CreateDataBlock(T subject, string principalId) => subject.ToDataBlock(principalId: principalId, blockType: _blockType);
//}
