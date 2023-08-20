using Toolbox.Block;

namespace SpinCluster.sdk.Serialization;

public struct BlockChainModelSerialization
{
    public IList<BlockNode> Blocks;
}


[RegisterConverter]
public sealed class BlockChainModelSerializationConverter : IConverter<BlockChainModel, BlockChainModelSerialization>
{
    public BlockChainModel ConvertFromSurrogate(in BlockChainModelSerialization surrogate) => new BlockChainModel
    {
        Blocks = surrogate.Blocks.ToList(),
    };

    public BlockChainModelSerialization ConvertToSurrogate(in BlockChainModel value) => new BlockChainModelSerialization
    {
        Blocks = value.Blocks.ToList(),
    };
}


