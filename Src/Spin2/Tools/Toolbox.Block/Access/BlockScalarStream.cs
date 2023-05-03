using Toolbox.Block.Container;
using Toolbox.Tools;
using Toolbox.Types.Maybe;

namespace Toolbox.Block.Access;

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

    public BlockScalarStream Add<T>(T value, string principleId) where T : class
    {
        _blockChain.Add(value, principleId, _blockType);
        return this;
    }

    public Option<T> Get<T>() where T : class => _blockChain
        .GetTypedBlocks<T>(_blockType)
        .LastOrDefaultOption();
}
