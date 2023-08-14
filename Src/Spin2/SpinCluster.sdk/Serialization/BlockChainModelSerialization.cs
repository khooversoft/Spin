using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Block;
using static System.Reflection.Metadata.BlobBuilder;

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


