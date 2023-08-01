//using Toolbox.Tools;
//using Toolbox.Types;

//namespace Toolbox.Block;

///// <summary>
///// Provides snapshot stream, last entry is current
///// 
///// BlockType = resource path : "{streamName}"
///// ObjectType = object type name (serialize / deserialize)
///// 
///// </summary>
//public class BlockScalarStream<T> where T : class
//{
//    private readonly BlockChain _blockChain;
//    private readonly string _streamName;
//    private readonly string _blockType;

//    public BlockScalarStream(BlockChain blockChain, string streamName)
//    {
//        _blockChain = blockChain.NotNull();
//        _streamName = streamName.NotNull();

//        _blockType = $"scalar:{_streamName.NotEmpty()}";
//    }

//    public Option Add(DataBlock value) => _blockChain.Add(value);

//    public Option<T> Get() => _blockChain
//        .GetTypedBlocks<T>(_blockType)
//        .LastOrDefaultOption();

//    public DataBlock CreateDataBlock(T subject, string principalId)
//    {
//        return subject.ToDataBlock(principalId, _blockType);
//    }
//}


//public static class BlockScalarStreamExtensions
//{
//    public static BlockScalarStream<T> GetScalarStream<T>(this BlockChain blockChain, string streamName) where T : class =>
//        new BlockScalarStream<T>(blockChain, streamName);
//}