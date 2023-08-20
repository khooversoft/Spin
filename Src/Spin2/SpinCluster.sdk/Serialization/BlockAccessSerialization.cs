using Toolbox.Block;

namespace SpinCluster.sdk.Serialization;

[GenerateSerializer]
public struct BlockAccessSerialization
{
    [Id(0)] public BlockGrant Grant;
    [Id(1)] public string? Claim;
    [Id(2)] public string BlockType;
    [Id(3)] public string PrincipalId;
}


[RegisterConverter]
public sealed class BlockAccessSerializationConverter : IConverter<BlockAccess, BlockAccessSerialization>
{
    public BlockAccess ConvertFromSurrogate(in BlockAccessSerialization surrogate) => new BlockAccess
    {
        Grant = surrogate.Grant,
        Claim = surrogate.Claim,
        BlockType = surrogate.BlockType,
        PrincipalId = surrogate.PrincipalId,
    };

    public BlockAccessSerialization ConvertToSurrogate(in BlockAccess value) => new BlockAccessSerialization
    {
        Grant = value.Grant,
        Claim = value.Claim,
        BlockType = value.BlockType,
        PrincipalId = value.PrincipalId,
    };
}
