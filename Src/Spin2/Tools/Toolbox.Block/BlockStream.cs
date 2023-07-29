using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Block;

/// <summary>
/// Provides multiple stream capability for block chains.
/// 
/// BlockType = resource path : "{streamName}"
/// ObjectType = object type name (serialize / deserialize)
/// 
/// </summary>
public class BlockStream<T> where T : class
{
    private readonly BlockChain _blockChain;
    private readonly string _streamName;
    private readonly string _blockType;

    public BlockStream(BlockChain blockChain, string streamName)
    {
        _blockChain = blockChain.NotNull();
        _streamName = streamName.NotNull();

        _blockType = $"collection:{_streamName.NotEmpty()}";
    }

    public Option Add(DataBlock value) => _blockChain.Add(value);

    public IReadOnlyList<T> Get() => _blockChain.GetTypedBlocks<T>(_blockType);

    public DataBlock CreateDataBlock(T subject, string principalId)
    {
        return subject.ToDataBlock(principalId, _blockType);
    }
}


public static class BlockStreamExtensions
{
    public static BlockStream<T> GetStream<T>(this BlockChain blockChain, string streamName) where T : class =>
        new BlockStream<T>(blockChain, streamName);
}