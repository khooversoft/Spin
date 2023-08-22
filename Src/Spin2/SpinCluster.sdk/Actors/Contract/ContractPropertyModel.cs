using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Block;

namespace SpinCluster.sdk.Actors.Contract;

public record ContractPropertyModel
{
    public string DocumentId { get; init; } = null!;
    public string OwnerPrincipalId { get; init; } = null!;
    public IReadOnlyList<BlockAccess> BlockAcl { get; init; } = Array.Empty<BlockAccess>();
    public int BlockCount { get; init; }
}
