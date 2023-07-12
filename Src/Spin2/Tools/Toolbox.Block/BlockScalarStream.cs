using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Block;

/// <summary>
/// Provides snapshot stream, last entry is current
/// 
/// BlockType = resource path : "{streamName}"
/// ObjectType = object type name (serialize / deserialize)
/// 
/// </summary>
public class BlockScalarStream
{
    private readonly BlockChain _blockChain;
    private readonly string _streamName;
    private readonly string _blockType;

    public BlockScalarStream(BlockChain blockChain, string streamName)
    {
        _blockChain = blockChain.NotNull();
        _streamName = streamName.NotNull();

        _blockType = $"scalar:{_streamName.NotEmpty()}";
    }

    public void Add(DataBlock value) => _blockChain.Add(value);

    public Option<T> Get<T>() where T : class => _blockChain
        .GetTypedBlocks<T>(_blockType)
        .LastOrDefaultOption();

    public DataBlock ToDataBlock<T>(T subject, string principalId) where T : class
    {
        return subject.ToDataBlock(principalId, _blockType);
    }
}


public static class BlockScalarStreamExtensions
{
    public static BlockScalarStream GetScalarStream(this BlockChain blockChain, string streamName) => new BlockScalarStream(blockChain, streamName);
}