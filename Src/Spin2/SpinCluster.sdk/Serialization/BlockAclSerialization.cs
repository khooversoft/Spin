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
        AccessRights = surrogate.Items.ToArray(),
    };

    public BlockAclSerialization ConvertToSurrogate(in BlockAcl value) => new BlockAclSerialization
    {
        Items = value.AccessRights.ToArray(),
    };
}
