using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Block;

namespace SpinCluster.sdk.Serialization;

[GenerateSerializer]
public struct BlockAclSerialization
{
    [Id(0)] public IReadOnlyList<BlockAccess> Items;
}


[RegisterConverter]
public sealed class BlockAclSerializationConverter : IConverter<BlockAcl, BlockAclSerialization>
{
    public BlockAcl ConvertFromSurrogate(in BlockAclSerialization surrogate) => new BlockAcl
    {
        Items = surrogate.Items.ToArray(),
    };

    public BlockAclSerialization ConvertToSurrogate(in BlockAcl value) => new BlockAclSerialization
    {
        Items = value.Items.ToArray(),
    };
}
