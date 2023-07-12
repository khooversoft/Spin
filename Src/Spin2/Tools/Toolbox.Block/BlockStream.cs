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
public class BlockStream
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

    public void Add(DataBlock value) => _blockChain.Add(value);

    public IReadOnlyList<T> Get<T>() where T : class => _blockChain.GetTypedBlocks<T>(_blockType);

    public DataBlock ToDataBlock<T>(T subject, string principalId) where T : class
    {
        return subject.ToDataBlock(principalId, _blockType);
    }
}


public static class BlockStreamExtensions
{
    public static BlockStream GetStream(this BlockChain blockChain, string streamName) => new BlockStream(blockChain, streamName);
}