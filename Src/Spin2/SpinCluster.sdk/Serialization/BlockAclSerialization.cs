using Toolbox.Block;

namespace SpinCluster.sdk.Serialization;

[GenerateSerializer]
public struct BlockAclSerialization
{
    [Id(0)] public IReadOnlyList<AccessBlock> Items;
}


[RegisterConverter]
public sealed class BlockAclSerializationConverter : IConverter<AclBlock, BlockAclSerialization>
{
    public AclBlock ConvertFromSurrogate(in BlockAclSerialization surrogate) => new AclBlock
    {
        AccessRights = surrogate.Items.ToArray(),
    };

    public BlockAclSerialization ConvertToSurrogate(in AclBlock value) => new BlockAclSerialization
    {
        Items = value.AccessRights.ToArray(),
    };
}
