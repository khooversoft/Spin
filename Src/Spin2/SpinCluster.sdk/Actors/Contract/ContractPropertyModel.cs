using Toolbox.Block;

namespace SpinCluster.sdk.Actors.Contract;

public record ContractPropertyModel
{
    public string DocumentId { get; init; } = null!;
    public string OwnerPrincipalId { get; init; } = null!;
    public IReadOnlyList<AccessBlock> BlockAcl { get; init; } = Array.Empty<AccessBlock>();
    public int BlockCount { get; init; }
}
