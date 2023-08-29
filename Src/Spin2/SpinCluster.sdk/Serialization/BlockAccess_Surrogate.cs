using Toolbox.Block;

namespace SpinCluster.sdk.Serialization;

[GenerateSerializer]
public struct BlockAccess_Surrogate
{
    [Id(0)] public BlockGrant Grant;
    [Id(1)] public string? Claim;
    [Id(2)] public string BlockType;
    [Id(3)] public string PrincipalId;
}


[RegisterConverter]
public sealed class BlockAccess_SurrogateConverter : IConverter<AccessBlock, BlockAccess_Surrogate>
{
    public AccessBlock ConvertFromSurrogate(in BlockAccess_Surrogate surrogate) => new AccessBlock
    {
        Grant = surrogate.Grant,
        Claim = surrogate.Claim,
        BlockType = surrogate.BlockType,
        PrincipalId = surrogate.PrincipalId,
    };

    public BlockAccess_Surrogate ConvertToSurrogate(in AccessBlock value) => new BlockAccess_Surrogate
    {
        Grant = value.Grant,
        Claim = value.Claim,
        BlockType = value.BlockType,
        PrincipalId = value.PrincipalId,
    };
}
