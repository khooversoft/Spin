//using Toolbox.Block.Container;
//using Toolbox.Tools;
//using Toolbox.Types;

//namespace Toolbox.Block.Access;

///// <summary>
///// Provides multiple stream capability for block chains.
///// 
///// BlockType = resource path : "{streamName}"
///// ObjectType = object type name (serialize / deserialize)
///// 
///// </summary>
//public class BlockCollectionStream
//{
//    private readonly BlockChain _blockChain;
//    private readonly string _streamName;
//    private readonly string _blockType;

//    public BlockCollectionStream(BlockChain blockChain, string streamName)
//    {
//        _blockChain = blockChain.NotNull();
//        _streamName = streamName.NotNull();

//        _blockType = $"collection:{_streamName.NotEmpty()}";
//    }

//    public string Add<T>(T value, string principleId) where T : class
//    {
//        string blockId = _blockChain.Add(value, principleId, _blockType);
//        return blockId;
//    }

//    public Option<IReadOnlyList<T>> Get<T>() where T : class => _blockChain
//        .GetTypedBlocks<T>(_blockType)
//        .ToOption();
//}
