using SpinCluster.sdk.Actors.Contract;
using Toolbox.Block;

namespace SpinCluster.sdk.Serialization;

[GenerateSerializer]
public struct ContractPropertyModel_Surrogate
{
    [Id(0)] public string DocumentId;
    [Id(1)] public string OwnerPrincipalId;
    [Id(2)] public IReadOnlyList<BlockAccess> BlockAcl;
    [Id(3)] public int BlockCount;
}

[RegisterConverter]
public sealed class ContractPropertyModel_SurrogateConverter : IConverter<ContractPropertyModel, ContractPropertyModel_Surrogate>
{
    public ContractPropertyModel ConvertFromSurrogate(in ContractPropertyModel_Surrogate surrogate) => new ContractPropertyModel
    {
        DocumentId = surrogate.DocumentId,
        OwnerPrincipalId = surrogate.OwnerPrincipalId,
        BlockAcl = surrogate.BlockAcl,
        BlockCount = surrogate.BlockCount,
    };

    public ContractPropertyModel_Surrogate ConvertToSurrogate(in ContractPropertyModel value) => new ContractPropertyModel_Surrogate
    {
        DocumentId = value.DocumentId,
        OwnerPrincipalId = value.OwnerPrincipalId,
        BlockAcl = value.BlockAcl,
        BlockCount = value.BlockCount,
    };
}
